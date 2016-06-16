using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ElasticCurl.Models;
using Elasticsearch.Net;
using Nest;

namespace ElasticCurl
{
    public class ElasticConnector
    {
        private const string SearchIndexName = "ts-search-index";
        private const string SuggestionIndexName = "ts-suggestion-index";
        private const string ClusterUri = "http://113121f8664ccf8c8a10dccd10132cd7.us-east-1.aws.found.io:9200/";

        public ElasticClient GetClient()
        {
            var nodes = new Uri[]
            {
                new Uri(ClusterUri)
            };

            var pool = new StaticConnectionPool(nodes);

            var settings = new ConnectionSettings(pool);


            settings.MapDefaultTypeIndices(
                x => x.Add(typeof(TsSuggestion), SuggestionIndexName));

            var client = new ElasticClient(settings);

            CreateIndexIfNotExists(client);

            return client;
        }

        public void IndexSuggestionDocument(IElasticClient client, TsSuggestion model)
        {
            var response = client.Index(model, idx => idx.Index(SuggestionIndexName));

            if (!response.IsValid)
            {
                throw new Exception("Could not index document to elastic", response.OriginalException);
            }
        }

        public void DeleteSuggestionIndexAndReCreate(IElasticClient client)
        {
            client.DeleteIndex(SuggestionIndexName);

            CreateSuggestionsIndexIfNotExists(client);
        }

        public void OptimizeSuggestionIndex(IElasticClient client)
        {
            client.Optimize(SuggestionIndexName);
        }


        public async Task<IEnumerable<TsSuggestion>> GetSuggestions(IElasticClient client, SuggestionRequest request)
        {
            var response = await client.SearchAsync<TsSuggestion>(s => s
                .Query(query => query.Term(x => x.Value, request.Query)));

            return response.Documents;
        }

        private void CreateIndexIfNotExists(IElasticClient client)
        {
            var searchIndexExists = client.IndexExists(SearchIndexName);

            if (!searchIndexExists.IsValid)
            {
                throw searchIndexExists.OriginalException;
            }

            if (!searchIndexExists.Exists)
            {
                var searchIndexCreateResponse = client.CreateIndex(SearchIndexName);

                if (!searchIndexCreateResponse.IsValid)
                {
                    throw searchIndexCreateResponse.OriginalException;
                }
            }

            var suggestionIndexExists = client.IndexExists(SuggestionIndexName);

            if (!suggestionIndexExists.IsValid)
            {
                throw suggestionIndexExists.OriginalException;
            }

            if (!suggestionIndexExists.Exists)
            {
                var suggestionIndexCreateResponse = CreateSuggestionsIndexIfNotExists(client);

                if (!suggestionIndexCreateResponse.IsValid)
                {
                    throw suggestionIndexCreateResponse.OriginalException;
                }
            }
        }

        private ICreateIndexResponse CreateSuggestionsIndexIfNotExists(IElasticClient client)
        {
            var suggestionAnalyzer = new CustomAnalyzer
            {
                Filter = new List<string> { "lowercase", "standard", "asciifolding" },
                Tokenizer = "allEdgeNGramTokenizer"
            };

            var requestAnalysis = new Analysis
            {
                Analyzers = new Analyzers
                {
                    {"suggestionAnalyzer", suggestionAnalyzer}
                },
                Tokenizers = new Tokenizers
                {
                    {
                        "allEdgeNGramTokenizer", new EdgeNGramTokenizer
                        {
                            MinGram = 1,
                            MaxGram = 12,
                            TokenChars =
                                new List<TokenChar>
                                {
                                    TokenChar.Digit,
                                    TokenChar.Letter,
                                    TokenChar.Punctuation,
                                    TokenChar.Symbol,
                                    TokenChar.Whitespace
                                }
                        }
                    }
                }
            };

            var indexDescriptor = new CreateIndexDescriptor(SuggestionIndexName)
                .Mappings(x => x.Map<TsSuggestion>(m => m.AutoMap()))
                .Settings(x => x.Analysis(m => requestAnalysis));

            return client.CreateIndex(indexDescriptor);
        }
    }
}