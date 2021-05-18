using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Elastic.Interfaces;
using Common.Elastic.Models;
using Newtonsoft.Json;
using RestSharp;

namespace Common.Elastic.Clients
{
    public class IndexClient : IIndexClient
    {
        private readonly IIndexSettings _indexSettings;

        private readonly IJsonHelper _jsonHelper;

        private readonly IRestClient _restClient;

        public IndexClient(
            IIndexSettings indexSettings,
            IRestClient restClient)
        {
            _jsonHelper = new JsonHelper();

            _indexSettings = indexSettings;

            _restClient = restClient;
        }

        public async Task PingAsync()
        {
            var requestUrl = _indexSettings.GetClusterHealthUri();

            var request = new RestRequest(requestUrl);

            var response = await _restClient.ExecuteAsync(request, Method.GET);

            if (!response.IsSuccessful)
            {
                throw new Exception("Failed to ping elastic");
            }
        }

        public async Task RegisterIndicesAsync()
        {
            await CreateSuggestionIndexIfNotExistsAsync();

            await CreateSearchIndexIfNotExistsAsync();
        }

        public async Task SeedSuggestionIndexAsync(SuggestionIndexRequest request)
        {
            var requestUrl = _indexSettings.GetSuggestionIndexUri(request.Id);

            var restRequest = new RestRequest(requestUrl).AddJsonBody(
                JsonConvert.SerializeObject(MapSuggestionIndexRequestToSuggestionIndex(request)));

            var response = await _restClient.ExecuteAsync(restRequest, Method.PUT);

            if (!response.IsSuccessful)
            {
                throw new Exception($"{Constants.PackageNameKey}: Failed to index document");
            }
        }

        public async Task SeedSuggestionsIndexAsync(List<SuggestionIndexRequest> requests)
        {
            var requestUrl = _indexSettings.GetSuggestionIndexForBulkIndexUri();

            var batchRequests = requests.Select(MapSuggestionIndexRequestToSuggestionIndex).ToList();

            var restRequest = new RestRequest(requestUrl).AddJsonBody(
                await _jsonHelper.CreateBulkJsonContentAsync(batchRequests));

            var response = await _restClient.ExecuteAsync(restRequest, Method.POST);

            if (!response.IsSuccessful)
            {
                throw new Exception($"{Constants.PackageNameKey}: Failed to index documents");
            }
        }

        public async Task SeedSearchIndexAsync(SearchIndexRequest request)
        {
            var requestUrl = _indexSettings.GetSearchIndexUri(request.Id);

            var restRequest = new RestRequest(requestUrl).AddJsonBody(
                JsonConvert.SerializeObject(MapSearchIndexRequestToSearchIndex(request)));

            var response = await _restClient.ExecuteAsync(restRequest, Method.PUT);

            if (!response.IsSuccessful)
            {
                throw new Exception($"{Constants.PackageNameKey}: Failed to index document");
            }
        }

        public async Task SeedSearchesIndexAsync(List<SearchIndexRequest> requests)
        {
            var requestUrl = _indexSettings.GetSearchIndexForBulkIndexUri();

            var batchRequests = requests.Select(MapSearchIndexRequestToSearchIndex).ToList();

            var restRequest = new RestRequest(requestUrl).AddJsonBody(
                await _jsonHelper.CreateBulkJsonContentAsync(batchRequests));

            var response = await _restClient.ExecuteAsync(restRequest, Method.POST);

            if (!response.IsSuccessful)
            {
                throw new Exception($"{Constants.PackageNameKey}: Failed to index documents");
            }
        }

        #region Internal/Private Members

        internal async Task<bool> CheckIfIndexExistsAsync(Uri indexUri)
        {
            var request = new RestRequest(indexUri);

            var response = await _restClient.ExecuteAsync(request, Method.HEAD);

            return response.IsSuccessful;
        }

        internal async Task CreateSuggestionIndexIfNotExistsAsync()
        {
            var requestUrl = _indexSettings.GetSuggestionIndexUri();

            var indexExists = await CheckIfIndexExistsAsync(requestUrl);

            if (indexExists)
            {
                return;
            }

            var request = new RestRequest(requestUrl).AddJsonBody(
                await _jsonHelper.GetJsonContentAsync(
                    $"{Constants.PackageNameKey}.Indices.SuggestionIndex.json"));

            var response = await _restClient.ExecuteAsync(request, Method.PUT);

            if (!response.IsSuccessful)
            {
                throw new Exception("Failed to register indices");
            }
        }

        internal async Task CreateSearchIndexIfNotExistsAsync()
        {
            var requestUrl = _indexSettings.GetSearchIndexUri();

            var indexExists = await CheckIfIndexExistsAsync(requestUrl);

            if (indexExists)
            {
                return;
            }

            var request = new RestRequest(requestUrl).AddJsonBody(
                await _jsonHelper.GetJsonContentAsync(
                    $"{Constants.PackageNameKey}.Indices.TemplateSearchIndex.json"));

            var response = await _restClient.ExecuteAsync(request, Method.PUT);

            if (!response.IsSuccessful)
            {
                throw new Exception("Failed to register indices");
            }
        }

        private static SuggestionIndex MapSuggestionIndexRequestToSuggestionIndex(SuggestionIndexRequest request)
        {
            return new SuggestionIndex
            {
                Id = request.Id,
                Value = request.Value,
                ValueType = request.ValueType,
                ImageUri = request.ImageUri,
                AfmcCode = request.AfmcCode,
                TemplateAuthorName = request.TemplateAuthorName,
                TemplateAuthorRole = request.TemplateAuthorRole,
                TemplateName = request.TemplateName,
                TemplateIsDeleted = request.TemplateIsDeleted,
                TemplateIsArticle = request.TemplateIsArticle,
                TemplateIsLandscape = request.TemplateIsLandscape,
                TemplateIsUploaded = request.TemplateIsUploaded
            };
        }

        private static SearchIndex MapSearchIndexRequestToSearchIndex(SearchIndexRequest request)
        {
            return new SearchIndex
            {
                Id = request.Id,
                Title = request.Title,
                EscapedTitle = request.EscapedTitle,
                Description = request.Description,
                TemplateUri = request.TemplateUri,
                TemplateAuthorName = request.TemplateAuthorName,
                TemplateAuthorId = request.TemplateAuthorId,
                TemplateCode = request.TemplateCode,
                TemplateAuthorSchoolDistrict = request.TemplateAuthorSchoolDistrict,
                TemplateAuthorAfmcCode = request.TemplateAuthorAfmcCode,
                TemplateIsFeatured = request.TemplateIsFeatured,
                TemplateIsDeleted = request.TemplateIsDeleted,
                TemplateIsArticle = request.TemplateIsArticle,
                TemplateIsLandscape = request.TemplateIsLandscape,
                TemplateIsUploaded = request.TemplateIsUploaded,
                TemplateInspiredTemplateId = request.TemplateInspiredTemplateId,
                TemplateInspiredAuthorName = request.TemplateInspiredAuthorName,
                TemplateInspiredTemplateAfmcCode = request.TemplateInspiredTemplateAfmcCode,
                TemplateSpellData = request.TemplateSpellData,
                TemplateInspiredAuthorUri = request.TemplateInspiredAuthorUri,
                TemplateInspiredAuthorAfmcCode = request.TemplateInspiredAuthorAfmcCode,
                ModifiedBy = request.ModifiedBy,
                TemplateDownloadCount = request.TemplateDownloadCount,
                TemplateClonedCount = request.TemplateClonedCount,
                TemplateSmileyCount = request.TemplateSmileyCount,
                TemplateTags = request.TemplateTags.Select(y => new TemplateTagIndex
                {
                    Id = y.Id,
                    TagGroupId = y.TagGroupId,
                    TagGroupName = y.TagGroupName,
                    TagValue = y.TagValue
                }).ToList()
            };
        }

        #endregion
    }
}
