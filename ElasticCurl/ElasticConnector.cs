using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
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
        private static readonly string ClusterUri = ConfigurationManager.AppSettings["ElasticClusterUri"];

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


        public async Task<SearchResults<TsSuggestion>> GetSuggestions(IElasticClient client, SuggestionRequest request)
        {
            // start watch
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // query string
            var queryString = request.Query.ToLower().Trim();

            var suggestions = await client.SearchAsync<TsSuggestion>(x => x
                .Size(request.PageSize)
                .From(request.PageSize*request.CurrentPage)
                .MinScore(request.MinScore)
                .Highlight(hd => hd
                    .PreTags("<b>")
                    .PostTags("</b>")
                    .Fields(fields => fields.Field("*")))
                .Query(query =>
                    query.Match(m1 => m1.Field(f1 => f1.Value).Query(queryString).Analyzer("suggestionAnalyzer")))
                .Sort(s => s.Descending("_score")));

            var response = new List<TsSuggestion>();

            foreach (var hit in suggestions.Hits)
            {
                var newSuggestion = hit.Source;
                newSuggestion.Score = hit.Score;

                response.Add(newSuggestion);
            }

            stopwatch.Stop();

            return new SearchResults<TsSuggestion>()
            {
                Results = response,
                Count = suggestions.Total,
                Query = suggestions.CallDetails.RequestBodyInBytes != null ? Encoding.UTF8.GetString(suggestions.CallDetails.RequestBodyInBytes) : null,
                Ticks = stopwatch.ElapsedTicks
            };
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
                Filter = new List<string> { "lowercase", "edgeNGram" },
                Tokenizer = "standard"
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
                        "edgeNGramTokenizer", new EdgeNGramTokenizer
                        {
                            MinGram = 1,
                            MaxGram = 12,
                            TokenChars =
                                new List<TokenChar>
                                {
                                    TokenChar.Digit,
                                    TokenChar.Letter,
                                    TokenChar.Whitespace
                                }
                        }
                    }
                },
                TokenFilters = new TokenFilters
                {
                    {
                        "nGram", new NGramTokenFilter
                        {
                            MinGram = 1,
                            MaxGram = 15
                        }
                    },

                    {
                        "edgeNGram", new EdgeNGramTokenFilter
                        {
                            MinGram = 1,
                            MaxGram = 15
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