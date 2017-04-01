﻿using System;
using System.Configuration;
using TsElasticIndexer;

namespace TsElasticIndexerTester
{
    public class Program
    {
        private static readonly string DocumentDbEndpointUrl = ConfigurationManager.AppSettings["DocumentDbEndpointUrl"];
        private static readonly string AuthorizationKey = ConfigurationManager.AppSettings["AuthorizationKey"];
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private static readonly string SuggestionCollectionId = ConfigurationManager.AppSettings["SuggestionCollectionId"];
        private static readonly string TemplateCollectionId = ConfigurationManager.AppSettings["TemplateCollectionId"];
        private static readonly string ElasticClusterUri = ConfigurationManager.AppSettings["ElasticClusterUri"];
        private static readonly string ElasticAdminUserName = ConfigurationManager.AppSettings["ElasticAdminUserName"];
        private static readonly string ElasticAdminPassword = ConfigurationManager.AppSettings["ElasticAdminPassword"];
        private static readonly int LastUpdatedTime = Convert.ToInt32(ConfigurationManager.AppSettings["LastUpdatedDate"]);
        private static readonly string TemplateIndexName = ConfigurationManager.AppSettings["TemplateIndexName"];
        private static readonly string SuggestIndexName = ConfigurationManager.AppSettings["SuggestIndexName"];

        static void Main(string[] args)
        {
            var processor = new Processor();
            processor.Start(new TsIndexerRequest
            {
                AuthorizationKey = AuthorizationKey,
                DatabaseId = DatabaseId,
                SuggestionCollectionId = SuggestionCollectionId,
                TemplateCollectionId = TemplateCollectionId,
                DocumentDbEndpointUrl = DocumentDbEndpointUrl,
                ElasticClusterUris = new []{ElasticClusterUri},
                ElasticAdminUserName = ElasticAdminUserName,
                ElasticAdminPassword = ElasticAdminPassword,
                LastUpdatedDate = LastUpdatedTime,
                SuggestIndexName = SuggestIndexName,
                TemplateIndexName = TemplateIndexName
            });
        }
    }
}
