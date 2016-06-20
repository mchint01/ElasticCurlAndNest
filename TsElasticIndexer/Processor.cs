using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElasticCommon;
using ElasticCommon.SearchModels;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace TsElasticIndexer
{
    public class Processor
    {
        private static DocumentClient _documentClient;

        public static void Start(TsIndexerRequest request)
        {
            using (_documentClient = new DocumentClient(new Uri(request.DocumentDbEndpointUrl), request.AuthorizationKey))
            {
                //ensure the database & collection exist before running samples
                Init(request);

                //get all suggestions changed in last 10 mins and update them
                UpdateSuggestionIndex(request);

                //get all templates changed in last x mins and update them
                UpdateTemplateIndex(request);
            }
        }

        private static void Init(TsIndexerRequest request)
        {
            GetDatabase(request.DatabaseId);

            GetCollection(request.DatabaseId, request.SuggestionCollectionId);
        }

        private static Database GetDatabase(string databaseId)
        {
            var database = _documentClient.CreateDatabaseQuery()
                .Where(db => db.Id == databaseId)
                .ToArray()
                .FirstOrDefault();

            if (database == null)
            {
                throw new Exception("Could not find Document Db database");
            }

            return database;
        }

        private static DocumentCollection GetCollection(string databaseId, string collectionId)
        {
            var databaseUri = UriFactory.CreateDatabaseUri(databaseId);

            var collection = _documentClient.CreateDocumentCollectionQuery(databaseUri)
                .Where(c => c.Id == collectionId)
                .AsEnumerable()
                .FirstOrDefault();

            if (collection == null)
            {
                throw new Exception("Could not find Document Db Collection");
            }

            return collection;
        }

        private static void UpdateSuggestionIndex(TsIndexerRequest request)
        {
            var elasticConnector = new ElasticConnector();

            var elasticClient = elasticConnector.GetClient(request.ElasticClusterUris,
                request.ElasticAdminUserName, request.ElasticAdminPassword);

            // form documentDb collection uri
            var collectionLink = UriFactory.CreateDocumentCollectionUri(request.DatabaseId, request.SuggestionCollectionId);
            var timeToGoBackFrom = DateTime.UtcNow.AddMinutes(-request.LastUpdatedDate).ToEpoch();

            //build up the query string
            var sql = string.Format("SELECT * FROM c where c._ts >= {0}", timeToGoBackFrom);

            //Get all the updated suggestions in last 10 mins
            var documents = _documentClient.CreateDocumentQuery(collectionLink, sql)
                .ToList();

            foreach (var d in documents)
            {
                TsSuggestion suggestion = JsonConvert.DeserializeObject<TsSuggestion>(d.ToString());

                if (suggestion.Deleted)
                {
                    //Delete the suggestion
                    elasticConnector.DeleteSuggestionDocument(elasticClient, suggestion);
                }
                else
                {
                    //Updating the suggestion index
                    elasticConnector.IndexSuggestionDocument(elasticClient, suggestion);
                }
            }
        }

        private static void UpdateTemplateIndex(TsIndexerRequest request)
        {
            var elasticConnector = new ElasticConnector();

            var elasticClient = elasticConnector.GetClient(request.ElasticClusterUris,
                request.ElasticAdminUserName, request.ElasticAdminPassword);

            // form documentDb collection uri
            var collectionLink = UriFactory.CreateDocumentCollectionUri(request.DatabaseId, request.TemplateCollectionId);
            var timeToGoBackFrom = DateTime.UtcNow.AddMinutes(--request.LastUpdatedDate).ToEpoch();

            //build up the query string
            var sql = string.Format("SELECT * FROM c where c._ts >= {0}", timeToGoBackFrom);

            //Get all the updated templates in last 10 mins
            var documents = _documentClient.CreateDocumentQuery(collectionLink, sql)
                .ToList();

            foreach (var d in documents)
            {
                TsTemplate template = JsonConvert.DeserializeObject<TsTemplate>(d.ToString());

                if (template.Deleted)
                {
                    //Delete the template
                    elasticConnector.DeleteTemplateDocument(elasticClient, template);
                }
                else
                {
                    //Updating the templates index
                    elasticConnector.IndexTemplateDocument(elasticClient, template);
                }
            }
        }
    }
}
