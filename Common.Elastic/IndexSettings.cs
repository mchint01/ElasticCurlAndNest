using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            throw new NotImplementedException();
        }

        public Uri GetSearchIndexUri(string documentId)
        {
            throw new NotImplementedException();
        }

        public Uri GetSuggestionIndexForBulkIndexUri()
        {
            throw new NotImplementedException();
        }

        public Uri GetSearchIndexForBulkIndexUri()
        {
            throw new NotImplementedException();
        }
    }
}
