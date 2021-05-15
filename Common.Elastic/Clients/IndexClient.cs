using System;
using System.Threading.Tasks;
using Common.Elastic.Interfaces;
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

        #region Internal Members

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

        #endregion
    }
}
