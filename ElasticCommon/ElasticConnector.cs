using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using ElasticCommon.Models;
using ElasticCommon.SearchModels;
using Elasticsearch.Net;
using Microsoft.Azure;
using Nest;
using SearchRequest = ElasticCommon.Models.SearchRequest;

namespace ElasticCommon
{
    public class ElasticConnector : IElasticConnector
    {
        private const string SearchIndexName = "ts-search-index";
        private const string SuggestionIndexName = "ts-suggestion-index";
        private static readonly string ClusterUri = CloudConfigurationManager.GetSetting("ElasticClusterUri");

        public ElasticClient GetClient()
        {
            var nodes = new Uri[]
            {
                new Uri(ClusterUri)
            };

            var pool = new StaticConnectionPool(nodes);

            var settings = new ConnectionSettings(pool);

            settings.BasicAuthentication(
                CloudConfigurationManager.GetSetting("ElasticAdminUserName"),
                CloudConfigurationManager.GetSetting("ElasticAdminPassword"));

            settings.MapDefaultTypeIndices(x =>
            {
                x.Add(typeof(TsSuggestion), SuggestionIndexName);
                x.Add(typeof(TsTemplate), SearchIndexName);
            });

            var client = new ElasticClient(settings);

            CreateIndexIfNotExists(client);

            return client;
        }

        #region Suggestion Public Members

        public void IndexSuggestionDocument(IElasticClient client, TsSuggestion model)
        {
            var response = client.Index(model, idx => idx.Index(SuggestionIndexName));

            if (!response.IsValid)
            {
                throw new Exception("Could not index document to elastic", response.OriginalException);
            }
        }

        public void DeleteSuggestionDocument(IElasticClient client, TsSuggestion model)
        {
            var response = client.Delete(new DeleteRequest(SuggestionIndexName, "ts_suggestion", model.Id));

            if (!response.IsValid)
            {
                throw new Exception("Could not delete document from elastic", response.OriginalException);
            }
        }

        public void DeleteSuggestionIndexAndReCreate(IElasticClient client)
        {
            client.DeleteIndex(SuggestionIndexName);

            CreateSuggestionsIndex(client);
        }

        public void OptimizeSuggestionIndex(IElasticClient client)
        {
            client.Optimize(SuggestionIndexName);
        }

        public async Task<SearchResults<TsSuggestion>> GetSuggestions(IElasticClient client, SearchRequest request)
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

        #endregion

        #region Template Search Public Members

        public void IndexTemplateDocument(IElasticClient client, TsTemplate model)
        {
            var response = client.Index(model, idx => idx.Index(SearchIndexName));

            if (!response.IsValid)
            {
                throw new Exception("Could not index document to elastic", response.OriginalException);
            }
        }

        public void DeleteTemplateDocument(IElasticClient client, TsTemplate model)
        {
            var response = client.Delete(new DeleteRequest(SuggestionIndexName, "ts_template", model.Id));

            if (!response.IsValid)
            {
                throw new Exception("Could not delete document from elastic", response.OriginalException);
            }
        }

        public void DeleteTemplateIndexAndReCreate(IElasticClient client)
        {
            client.DeleteIndex(SearchIndexName);

            CreateTemplateIndex(client);
        }

        public void OptimizeTemplateIndex(IElasticClient client)
        {
            client.Optimize(SearchIndexName);
        }

        public async Task<SearchResults<TsTemplate>> GetTemplates(IElasticClient client, SearchRequest request)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var templates = await client.SearchAsync<TsTemplate>(x =>
            {
                x.Size(request.PageSize)
                    .From(request.PageSize*request.CurrentPage)
                    .MinScore(request.MinScore)
                    .Highlight(hd => hd
                        .PreTags("<b>")
                        .PostTags("</b>")
                        .Fields(fields => fields.Field("*")));

                var baseQuery =
                    Query<TsTemplate>.Bool(b => b.Must(mbox => mbox.MatchAll()).Filter(ff => ff.Term("deleted", "0")));

                if (!string.IsNullOrEmpty(request.Query))
                {
                    var queryString = request.Query.ToLower().Trim();

                    baseQuery = 
                        Query<TsTemplate>.Match(
                            m1 => m1.Field(f1 => f1.Title).Query(queryString).Analyzer("suggestionAnalyzer")) ||
                        Query<TsTemplate>.Match(
                            m2 => m2.Field(f2 => f2.Desc).Query(queryString).Analyzer("suggestionAnalyzer")) ||
                        Query<TsTemplate>.Match(
                            m3 => m3.Field(f3 => f3.By).Query(queryString).Analyzer("suggestionAnalyzer")) ||
                        Query<TsTemplate>.Match(
                            m2 => m2.Field(f2 => f2.SchlDist).Query(queryString).Analyzer("suggestionAnalyzer")) ||
                        Query<TsTemplate>.Match(
                            m2 => m2.Field(f2 => f2.TmplTags).Query(queryString).Analyzer("suggestionAnalyzer")) ||
                        Query<TsTemplate>.Match(
                            m2 => m2.Field(f2 => f2.TmplTypes).Query(queryString).Analyzer("suggestionAnalyzer")) ||
                        Query<TsTemplate>.Match(
                            m2 => m2.Field(f2 => f2.InsAuthor).Query(queryString).Analyzer("suggestionAnalyzer")) ||
                        Query<TsTemplate>.Match(
                            m2 => m2.Field(f2 => f2.TmplCode).Query(queryString).Analyzer("suggestionAnalyzer"));
                }

                x.Query(q => baseQuery);

                x.Sort(s => s.Descending("_score").Descending("lstDt"));

                return x;
            });

            var response = new List<TsTemplate>();

            foreach (var hit in templates.Hits)
            {
                var newTemplate = hit.Source;
                newTemplate.Score = hit.Score;

                response.Add(newTemplate);
            }

            stopwatch.Stop();

            return new SearchResults<TsTemplate>()
            {
                Results = response,
                Count = templates.Total,
                Query =
                    templates.CallDetails.RequestBodyInBytes != null
                        ? Encoding.UTF8.GetString(templates.CallDetails.RequestBodyInBytes)
                        : null,
                Ticks = stopwatch.ElapsedTicks
            };
        }

        #endregion

        #region Private Members

        private void CreateIndexIfNotExists(IElasticClient client)
        {
            var searchIndexExists = client.IndexExists(SearchIndexName);

            if (!searchIndexExists.IsValid)
            {
                throw searchIndexExists.OriginalException;
            }

            if (!searchIndexExists.Exists)
            {
                var searchIndexCreateResponse = CreateTemplateIndex(client);

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
                var suggestionIndexCreateResponse = CreateSuggestionsIndex(client);

                if (!suggestionIndexCreateResponse.IsValid)
                {
                    throw suggestionIndexCreateResponse.OriginalException;
                }
            }
        }

        private ICreateIndexResponse CreateSuggestionsIndex(IElasticClient client)
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

        private ICreateIndexResponse CreateTemplateIndex(IElasticClient client)
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
                                    TokenChar.Whitespace,
                                    TokenChar.Symbol,
                                    TokenChar.Punctuation
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

            var indexDescriptor = new CreateIndexDescriptor(SearchIndexName)
                .Mappings(x => x.Map<TsTemplate>(m => m.AutoMap()))
                .Settings(x => x.Analysis(m => requestAnalysis));

            return client.CreateIndex(indexDescriptor);
        }

        #endregion
    }
}