using System;
using Common.Elastic.Interfaces;

namespace Common.Elastic
{
    public class IndexSettings : IIndexSettings
    {
        private readonly IConfigurationSettings _configurationSettings;

        public IndexSettings(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
        }

        public Uri GetClusterHealthUri()
        {
            return Uri.TryCreate(new Uri(_configurationSettings.ElasticBaseUrl), "/_cluster/health", out var requestUrl)
                ? requestUrl : null;
        }

        public Uri GetSuggestionIndexUri()
        {
            if (Uri.TryCreate(new Uri(_configurationSettings.ElasticBaseUrl), $"/{_configurationSettings.SuggestionIndexName}", out var requestUrl))
                return requestUrl;

            throw new Exception($"{Constants.PackageNameKey}: Failed to register indices");
        }

        public Uri GetSearchIndexUri()
        {
            if (Uri.TryCreate(new Uri(_configurationSettings.ElasticBaseUrl), $"/{_configurationSettings.SearchIndexName}", out var requestUrl))
                return requestUrl;

            throw new Exception($"{Constants.PackageNameKey}: Failed to register indices");
        }

        public Uri GetSuggestionIndexUri(string documentId)
        {
            var requestUri = GetSuggestionIndexUri();

            return new Uri($"{requestUri}/_doc/{documentId}?refresh=wait_for");
        }

        public Uri GetSearchIndexUri(string documentId)
        {
            var requestUri = GetSearchIndexUri();

            return new Uri($"{requestUri}/_doc/{documentId}?refresh=wait_for");
        }

        public Uri GetSuggestionIndexForBulkIndexUri()
        {
            var requestUri = GetSuggestionIndexUri();

            return new Uri($"{requestUri}/_bulk?refresh=wait_for");
        }

        public Uri GetSearchIndexForBulkIndexUri()
        {
            var requestUri = GetSearchIndexUri();

            return new Uri($"{requestUri}/_bulk?refresh=wait_for");
        }
    }
}
