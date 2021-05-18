using Common.Elastic.Interfaces;

namespace Common.Elastic
{
    public class ConfigurationSettings : IConfigurationSettings
    {
        public string ElasticBaseUrl { get; }

        public string SuggestionIndexName { get; }

        public string SearchIndexName { get; }

        public ConfigurationSettings(string elasticBaseUrl,
            string suggestionIndexName,
            string searchIndexName)
        {
            ElasticBaseUrl = elasticBaseUrl;

            SuggestionIndexName = suggestionIndexName;

            SearchIndexName = searchIndexName;
        }
    }
}
