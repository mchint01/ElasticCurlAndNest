using System;
using Common.Elastic.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Common.Elastic
{
    public class ConfigurationSettings : IConfigurationSettings
    {
        public string ElasticBaseUrl { get; }

        public string SuggestionIndexName { get; }

        public string SearchIndexName { get; }

        public ConfigurationSettings(IConfiguration configuration)
        {
            var searchConfigurationSection = configuration.GetSection("SearchSetting");

            ElasticBaseUrl = GetElasticBaseUri(searchConfigurationSection);

            SuggestionIndexName = GetElasticSuggestionIndexName(searchConfigurationSection);

            SearchIndexName = GetElasticSearchIndexName(searchConfigurationSection);
        }

        private static string GetElasticBaseUri(IConfiguration configurationSection)
        {
            var value = Environment.GetEnvironmentVariable(Constants.ElasticClusterUriKey);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.ToLower();
            }

            value = configurationSection["Elastic:BaseUri"];

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.ToLower();
            }

            throw new ArgumentNullException(
                $"{Constants.PackageNameKey}: {Constants.ElasticClusterUriKey} not found");
        }

        private static string GetElasticSuggestionIndexName(IConfiguration configurationSection)
        {
            var value = Environment.GetEnvironmentVariable(Constants.ElasticSuggestionIndexNameKey);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.ToLower();
            }

            value = configurationSection["Elastic:Index:Suggestion"];

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.ToLower();
            }

            throw new ArgumentNullException(
                $"{Constants.PackageNameKey}: {Constants.ElasticSuggestionIndexNameKey} not found");
        }

        private static string GetElasticSearchIndexName(IConfiguration configurationSection)
        {
            var value = Environment.GetEnvironmentVariable(Constants.ElasticSearchIndexNameKey);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.ToLower();
            }

            value = configurationSection["Elastic:Index:Search"];

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.ToLower();
            }

            throw new ArgumentNullException(
                $"{Constants.PackageNameKey}: {Constants.ElasticSearchIndexNameKey} not found");
        }
    }
}
