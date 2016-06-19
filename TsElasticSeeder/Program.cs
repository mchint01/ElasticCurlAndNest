using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ElasticCommon;
using ElasticCommon.Models;
using Microsoft.Azure;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace TsElasticSeeder
{
    public class Program
    {
        private static readonly string DocumentDbEndpointUrl = CloudConfigurationManager.GetSetting("DocumentDbEndpointUrl");
        private static readonly string AuthorizationKey = CloudConfigurationManager.GetSetting("AuthorizationKey");
        private static readonly string DatabaseId = CloudConfigurationManager.GetSetting("DatabaseId");
        private static readonly string SuggestionCollectionId = CloudConfigurationManager.GetSetting("SuggestionCollectionId");
        private static readonly string TemplateCollectionId = CloudConfigurationManager.GetSetting("TemplateCollectionId");

        private static DocumentClient _documentClient;

        static void Main(string[] args)
        {
            using (_documentClient = new DocumentClient(new Uri(DocumentDbEndpointUrl), AuthorizationKey))
            {
                //ensure the database & collection exist before running samples
                Init();

                Console.WriteLine("Seed Suggestions (Y/N)?");

                var seedSuggestions = Console.ReadLine();

                Console.WriteLine("Seed Templates (Y/N)?");

                var seedTemplates = Console.ReadLine();

                //get all collection data from document db & seed to elastic
                Seed(string.Equals(seedSuggestions, "Y", StringComparison.OrdinalIgnoreCase),
                    string.Equals(seedTemplates, "Y", StringComparison.OrdinalIgnoreCase));
            }
        }

        private static void Init()
        {
            GetDatabase(DatabaseId);

            GetCollection(DatabaseId, SuggestionCollectionId);

            GetCollection(DatabaseId, TemplateCollectionId);
        }

        private static void Seed(bool seedSuggestions, bool seedTemplates)
        {
            if (seedSuggestions)
            {
                var start = DateTime.Now;

                Console.WriteLine("Starting to Seed Suggestions");

                GetAllSuggestionsFromCollectionAndSeedToElastic(DatabaseId, SuggestionCollectionId).Wait();

                Console.WriteLine("Completed to Seed Suggestions");

                var end = DateTime.Now;

                Console.WriteLine("Time took to Seed Suggestions in Seconds {0}", (end - start).Seconds);
            }

            if (seedTemplates)
            {
                var start = DateTime.Now;

                Console.WriteLine("Starting to Seed Templates");

                GetAllTemplatesFromCollectionAndSeedToElastic(DatabaseId, TemplateCollectionId).Wait();

                Console.WriteLine("Completed to Seed Templates");

                var end = DateTime.Now;

                Console.WriteLine("Time took to Seed Templates in Seconds {0}", (end - start).Seconds);
            }

            Console.ReadLine();
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

        private static async Task GetAllSuggestionsFromCollectionAndSeedToElastic(string databaseId, string collectionId)
        {
            var elasticConnector = new ElasticConnector();

            var elasticClient = elasticConnector.GetClient();

            // delete and re-create elastic index
            elasticConnector.DeleteSuggestionIndexAndReCreate(elasticClient);

            // form documentDb collection uri
            var collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

            var continuation = string.Empty;
            do
            {
                // Read the feed 100 items at a time until there are no more items to read
                var response =
                    await _documentClient.ReadDocumentFeedAsync(collectionLink,
                        new FeedOptions
                        {
                            MaxItemCount = 100,
                            RequestContinuation = continuation
                        });

                // Append the item count

                // Get the continuation so that we know when to stop.
                continuation = response.ResponseContinuation;

                foreach (var d in response)
                {
                    var model = JsonConvert.DeserializeObject<TsSuggestion>(d.ToString());

                    // Seed data to elastic
                    elasticConnector.IndexSuggestionDocument(elasticClient, model);
                }

            } while (!string.IsNullOrEmpty(continuation));

            // optimize the suggestion index
            elasticConnector.OptimizeSuggestionIndex(elasticClient);
        }

        private static async Task GetAllTemplatesFromCollectionAndSeedToElastic(string databaseId, string collectionId)
        {
            var elasticConnector = new ElasticConnector();

            var elasticClient = elasticConnector.GetClient();

            // delete and re-create elastic index
            elasticConnector.DeleteTemplateIndexAndReCreate(elasticClient);

            // form documentDb collection uri
            var collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

            var continuation = string.Empty;
            do
            {
                // Read the feed 100 items at a time until there are no more items to read
                var response =
                    await _documentClient.ReadDocumentFeedAsync(collectionLink,
                        new FeedOptions
                        {
                            MaxItemCount = 100,
                            RequestContinuation = continuation
                        });

                // Get the continuation so that we know when to stop.
                continuation = response.ResponseContinuation;

                foreach (var d in response)
                {
                    var model = JsonConvert.DeserializeObject<TsTemplate>(d.ToString());

                    // Seed data to elastic
                    elasticConnector.IndexTemplateDocument(elasticClient, model);
                }

            } while (!string.IsNullOrEmpty(continuation));

            // optimize the suggestion index
            elasticConnector.OptimizeTemplateIndex(elasticClient);
        }

    }
}
