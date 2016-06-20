using System;
using System.Configuration;
using System.Linq;
using ElasticCommon;
using ElasticCommon.SearchModels;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using TsElasticIndexer;

namespace TsElasticIndexerApp
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
                ElasticAdminPassword = ElasticAdminPassword
            });
        }
    }
}
