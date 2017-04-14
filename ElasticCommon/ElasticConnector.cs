using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ElasticCommon.Models;
using ElasticCommon.SearchModels;
using Elasticsearch.Net;
using Nest;
using SearchRequest = ElasticCommon.Models.SearchRequest;

namespace ElasticCommon
{
    public class ElasticConnector : IElasticConnector
    {
        public ElasticConnector()
        {
        }

        public ElasticConnector(string searchIndexName, string suggestIndexName)
        {
            this.SearchIndexName = searchIndexName;
            this.SuggestionIndexName = suggestIndexName;
        }

        private string SearchIndexName = "ts-search-index";
        private string SuggestionIndexName = "ts-suggestion-index";

        public ElasticClient GetClient(string[] clusterUris, string userName, string password)
        {

    

            var nodes = clusterUris.Select(x => new Uri(x)).ToArray();

            var pool = new StaticConnectionPool(nodes);

            var settings = new ConnectionSettings(pool);
            // settings.EnableDebugMode(x => Console.WriteLine(Encoding.UTF8.GetString(x.RequestBodyInBytes)));

            settings.BasicAuthentication(userName, password);

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

        public bool CheckSuggestionDocumentExists(IElasticClient client, string id)
        {
            var response = client.Get<TsSuggestion>(new GetRequest(SuggestionIndexName, "ts_suggestion", id));

            return response.IsValid;
        }

        public void DeleteSuggestionIndexAndReCreate(IElasticClient client)
        {
            client.DeleteIndex(SuggestionIndexName);

            CreateSuggestionsIndex(client);
        }

        public void OptimizeSuggestionIndex(IElasticClient client)
        {
            client.ForceMerge(SuggestionIndexName);
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
                .From(request.PageSize * request.CurrentPage)
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
                newSuggestion.Score = hit.Score ?? 1.0;

                response.Add(newSuggestion);
            }

            stopwatch.Stop();

            return new SearchResults<TsSuggestion>()
            {
                Results = response,
                Count = suggestions.Total,
                Query = suggestions.ApiCall.RequestBodyInBytes != null ? Encoding.UTF8.GetString(suggestions.ApiCall.RequestBodyInBytes) : null,
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
            var response = client.Delete(new DeleteRequest(SearchIndexName, "ts_template", model.Id));

            if (!response.IsValid)
            {
                throw new Exception("Could not delete document from elastic", response.OriginalException);
            }
        }

        public bool CheckTemplateDocumentExists(IElasticClient client, string id)
        {
            var response = client.Get<TsTemplate>(new GetRequest(SearchIndexName, "ts_template", id));

            return response.IsValid;
        }

        public void DeleteTemplateIndexAndReCreate(IElasticClient client)
        {
            client.DeleteIndex(SearchIndexName);

            CreateTemplateIndex(client);
        }

        public void OptimizeTemplateIndex(IElasticClient client)
        {
            client.ForceMerge(SearchIndexName);
        }

        public async Task<SearchResults<TsTemplate>> GetTemplates(IElasticClient client, SearchRequest request)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (request.Query == null || request.Query == " ")
            {
                request.Query = String.Empty;
            }

            var queryString = request.Query.Trim();

            const string pat = @"\w{3}-\d+-\d+";
            var r = new Regex(pat, RegexOptions.IgnoreCase);

            if (r.Match(queryString).Success)
            {
                var codeTemplate = await client.SearchAsync<TsTemplate>(x =>
                {
                    var baseQuery =
                        Query<TsTemplate>.Bool(
                            b =>
                                b.Must(mbox => mbox.Term("tmplCode", queryString))
                                    .Filter(fff => fff.Term("deleted", "0")));

                    x.Query(q => baseQuery);

                    x.Sort(s => s.Descending("lstDt"));

                    return x;
                });

                if (codeTemplate.Hits.Any())
                {
                    return HandlingTemplateResults(codeTemplate, stopwatch);
                }
            }

            var templates = await client.SearchAsync<TsTemplate>(x =>
            {
                var filterMap = request.Filters;

                var mustFilters = new List<Func<QueryContainerDescriptor<TsTemplate>, QueryContainer>>();

                // must deleted is 0
                mustFilters.Add(fq => fq.Term(tf => tf.Deleted, 0));

                // must each filter
                foreach (var oneFilter in filterMap.AsEnumerable())
                {
                    mustFilters.Add(fq => fq.Terms(tf => tf.Field(oneFilter.Key).Terms(oneFilter.Value)));
                }


                // should each contributing clause
                var shouldQueries = new List<Func<QueryContainerDescriptor<TsTemplate>, QueryContainer>>();

                shouldQueries.Add(fq => fq.MultiMatch(mm => mm
                                                               .Query(queryString)
                                                                .Name("phraseTitleSlop2")
                                                               .Type(TextQueryType.Phrase)
                                                               .Slop(2)
                                                               .Boost(2)
                                                               .Fields(fl => fl.Field(f => f.Title.Suffix("base")).Field(f => f.Title.Suffix("nostop")))
                                                               .Operator(Operator.And)
                                                     ));
                shouldQueries.Add(fq => fq.MultiMatch(mm => mm
                                                               .Query(queryString)
                                                               .Name("phraseDescSlop2")
                                                               .Type(TextQueryType.Phrase)
                                                               .Slop(2)
                                                               .Boost(1.5)
                                                               .Fields(fl => fl.Field(f => f.Desc.Suffix("base")).Field(f => f.Desc.Suffix("nostop")))
                                                               .Operator(Operator.And)
                                                     ));
                shouldQueries.Add(fq => fq.MultiMatch(mm => mm
                                                           .Query(queryString)
                                                           .Name("mmCrossNoStop")
                                                           .Type(TextQueryType.CrossFields)
                                                           .Boost(1.2)
                                                           .Fields(fl => fl
                                                                   .Field(f => f.TmplTags.Suffix("nostop"), 6.0)
                                                                    .Field(f => f.Title.Suffix("nostop"), 4.0)
                                                                    .Field(f => f.Desc.Suffix("nostop"), 2.0)
                                                                    .Field(f => f.By.Suffix("nostop"), 1.0)
                                                                   .Field(f => f.TagGradeLevel.Suffix("nostop"), 4.0))
                                                           .Operator(Operator.And)
                                                           .TieBreaker(0)
                                                     ));
                shouldQueries.Add(fq => fq.MultiMatch(mm => mm
                                                           .Query(queryString)
                                                           .Name("mmCrossBase")
                                                           .Type(TextQueryType.CrossFields)
                                                           .Boost(1.0)
                                                           .Fields(fl => fl
                                                                    .Field(f => f.TmplTags.Suffix("base"), 6.0)
                                                                    .Field(f => f.Title.Suffix("base"), 4.0)
                                                                    .Field(f => f.Desc.Suffix("base"), 2.0)
                                                                    .Field(f => f.By.Suffix("base"), 1.0)
                                                                   .Field(f => f.TagGradeLevel.Suffix("base"), 4.0))
                                                           .Operator(Operator.And)
                                                           .TieBreaker(0)
                                                     ));

                var innerQuery = Query<TsTemplate>.Bool(bq => bq.Filter(mustFilters).Should(shouldQueries));

                // wrap with function score for date recency
                var recencyBoostFunctionScore = Query<TsTemplate>.FunctionScore(fs => fs
                                                            .Functions(fl => fl
                                                                       .GaussDate(g => g.Field(f => f.LstDt).Origin("now").Offset("1d").Scale("7d").Decay(0.75))
                                                                       .GaussDate(g => g.Field(f => f.LstDt).Origin("now").Offset("7d").Scale("30d").Decay(0.75))
                                                                       .GaussDate(g => g.Field(f => f.LstDt).Origin("now").Offset("30d").Scale("180d").Decay(0.75))
                                                                      )
                                                            .ScoreMode(FunctionScoreMode.Sum)
                                                            .BoostMode(FunctionBoostMode.Multiply)
                                                            .Query(q => innerQuery)
                                                           );

                // wrap with function score for smiley count
                var smileyBoostFunctionScore = new QueryContainerDescriptor<TsTemplate>()
                    .FunctionScore(fs => fs
                                   .Functions(fl => fl.FieldValueFactor(fvf => fvf
                                                                        .Field(f => f.SmileyCnt)
                                                                        .Factor(1.0)
                                                                        .Modifier(FieldValueFactorModifier.Ln2P)
                                                                        .Missing(0.69).Weight(1.0)))
                                   .ScoreMode(FunctionScoreMode.Multiply)
                                   .BoostMode(FunctionBoostMode.Multiply)
                                   .Query(q => null && recencyBoostFunctionScore)
                                  );

                var outerMostQuery = smileyBoostFunctionScore;


                x.Size(request.PageSize).From(request.PageSize * request.CurrentPage);

                x.MinScore(request.MinScore);

                x.Highlight(hd => hd
                    .PreTags("<b>")
                    .PostTags("</b>")
                           .Fields(fields => fields
                                   .Field(f => f.Title).FragmentSize(80).NumberOfFragments(1).NoMatchSize(80).MatchedFields(mf => mf
                                                                                                                            .Field(mff => mff.Title.Suffix("base"))
                                                                                                                            .Field(mff => mff.Title.Suffix("nostop"))),
                                   fields => fields
                                   .Field(f => f.Desc).FragmentSize(80).NumberOfFragments(1).NoMatchSize(80).MatchedFields(mf => mf
                                                                                                                            .Field(mff => mff.Desc.Suffix("base"))
                                                                                                                            .Field(mff => mff.Desc.Suffix("nostop")))
                                  ));

		if (request.Order == SearchOrdering.popular)
		{
		     x.Sort(s => s.Descending(f => f.SmileyCnt).Descending("_score"));	
		} else if (request.Order == SearchOrdering.recent) { 
		     x.Sort(s => s.Descending(f => f.LstDt).Descending("_score"));
		} else {
		     x.Sort(s => s.Descending("_score").Descending(f => f.LstDt));	
		}

                x.Query(q => outerMostQuery);

                Console.WriteLine(client.Serializer.SerializeToString(x));
                return x;
            });
            return HandlingTemplateResults(templates, stopwatch);
        }

        private SearchResults<TsTemplate> HandlingTemplateResults(ISearchResponse<TsTemplate> templates, Stopwatch stopwatch)
        {
            var response = new List<TsTemplate>();

            foreach (var hit in templates.Hits)
            {
                var newTemplate = hit.Source;
                newTemplate.Score = hit.Score ?? 1.0;

                response.Add(newTemplate);
            }
            stopwatch.Stop();

            return new SearchResults<TsTemplate>()
            {
                Results = response,
                Count = templates.Total,
                Query =
                    templates.ApiCall.RequestBodyInBytes != null
                        ? Encoding.UTF8.GetString(templates.ApiCall.RequestBodyInBytes)
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

            var en_base_analysis = new CustomAnalyzer
            {
                Filter = new List<string> { "icu_normalizer", "icu_folding", "grade_level_query_synonym_filter", "keyword_repeat", "en_possessive_stemmer_filter", "en_stop_filter", "en_stemmer_filter", "unique_same_position_filter" },
                Tokenizer = "icu_tokenizer"
            };

            var en_base_nostop_analysis = new CustomAnalyzer
            {
                Filter = new List<string> { "icu_normalizer", "icu_folding", "grade_level_query_synonym_filter", "keyword_repeat", "en_possessive_stemmer_filter", "en_stemmer_filter", "unique_same_position_filter" },
                Tokenizer = "icu_tokenizer"
            };

            var en_ngram_analysis = new CustomAnalyzer
            {
                Filter = new List<string> { "icu_normalizer", "icu_folding", "edgeNGram" },
                Tokenizer = "icu_tokenizer"
            };

            var grade_level_query_synonym_analysis = new CustomAnalyzer
            {
                Filter = new List<string> { "icu_normalizer", "icu_folding", "grade_level_query_synonym_filter" },
                Tokenizer = "icu_tokenizer"
            };

            var grade_level_index_synonym_analysis = new CustomAnalyzer
            {
                Filter = new List<string> { "icu_normalizer", "icu_folding", "grade_level_index_synonym_filter" },
                Tokenizer = "keyword"
            };

            var keyword_analyzer = new CustomAnalyzer
            {
                Filter = new List<string> { "icu_normalizer", "icu_folding" },
                Tokenizer = "keyword"
            };


            var base_normalizer = new CustomNormalizer
            {
                Filter = new List<string> { "icu_normalizer", "icu_folding" }
            };

            var requestAnalysis = new Analysis
            {
                Analyzers = new Analyzers
                {
                    {"keyword_analyzer", keyword_analyzer},
                    {"en_base_analysis", en_base_analysis},
                    {"en_base_nostop_analysis", en_base_nostop_analysis },
                    {"en_ngram_analysis", en_ngram_analysis },
                    {"grade_level_query_synonym_analysis", grade_level_query_synonym_analysis},
                    {"grade_level_index_synonym_analysis", grade_level_index_synonym_analysis}
                },
                Normalizers = new Normalizers
                {
                    {"base_normalizer", base_normalizer }
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
                        "en_stop_filter", new StopTokenFilter
                        {
                            StopWords = "_english_"
                        }
                    },
                    {
                        "en_stemmer_filter", new StemmerTokenFilter
                        {
                            Language = "minimal_english"
                        }
                    },
                    {
                        "en_possessive_stemmer_filter", new StemmerTokenFilter
                        {
                            Language = "possessive_english"
                        }
                    },
                    {
                        "unique_same_position_filter", new UniqueTokenFilter
                        {
                            OnlyOnSamePosition = true
                        }
                    },
                    {
                        "grade_level_index_synonym_filter", new SynonymTokenFilter
                        {
                            Synonyms = new List<String>
                            {
                                 "1 => GRADE1",
                                 "2 => GRADE2",
                                 "3 => GRADE3",
                                 "4 => GRADE4",
                                 "5 => GRADE5",
                                 "6 => GRADE6",
                                 "7 => GRADE7",
                                 "8 => GRADE8",
                                 "9 => GRADE9",
                                 "10 => GRADE10",
                                 "11 => GRADE11",
                                 "12 => GRADE12",
                                 "0 => GRADEK",
                                 "-1 => GRADEPK"
                            }
                        }
                    },
                    {
                        "grade_level_query_synonym_filter", new SynonymTokenFilter
                        {
                            Synonyms = new List<String>
                            {
                                 "first grade, 1st grade, grade 1, grade one => GRADE1",
                                 "second grade, 2nd grade, grade 2, grade two => GRADE2",
                                 "third grade, 3rd grade, grade 3, grade three => GRADE3",
                                 "fourth grade, 4th grade, grade 4, grade four => GRADE4",
                                 "fifth grade, 5th grade, grade 5, grade five => GRADE5",
                                 "sixth grade, 6th grade, grade 6, grade six => GRADE6",
                                 "seventh grade, 7th grade, grade 7, grade seven => GRADE7",
                                 "eighth grade, 8th grade, grade 8, grade eight => GRADE8",
                                 "nineth grade, 9th grade, grade 9, grade nine => GRADE9",
                                 "tenth grade, 10th grade, grade 10, grade ten => GRADE10",
                                 "eleventh grade, 11th grade, grade 11, grade eleven => GRADE11",
                                 "twelveth grade, 12th grade, grade 12, grade twelve => GRADE12",
                                 "k, kinder, kindergarten, kindergarden => GRADEK",
                                 "pk, preschool, prekindergarten, pre kindergarten, prekindergarden, pre kindergarden, pre-k => GRADEPK"
                            }
                        }
                    },
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
                .Mappings(x => x.Map<TsTemplate>(m =>
                                                 m.AutoMap()
                                                 .Properties(ps => ps
                                                             .Text(s => s.Name(e => e.Title).multiFields().CopyTo(c => c.Field(cf => cf.SpellData)))
                                                             .Text(s => s.Name(e => e.Desc).multiFields().CopyTo(c => c.Field(cf => cf.SpellData)))
                                                             .Text(s => s.Name(e => e.By).multiFields().CopyTo(c => c.Field(cf => cf.SpellData)))
                                                             .Text(s => s.Name(e => e.SchlDist).multiFields())
                                                             .Text(s => s.Name(e => e.TmplTags).multiFields().CopyTo(c => c.Field(cf => cf.SpellData)))
                                                             .Text(s => s.Name(e => e.TmplCcss).multiFields())
                                                             .Text(s => s.Name(e => e.TmplTypes).multiFields())
                                                             .Text(s => s.Name(e => e.InsAuthor).multiFields())
                                                             .Text(s => s.Name(e => e.SpellData).multiFields())
                                                             .Keyword(s => s.Name(e => e.TagGradeLevel).Fields(fs => fs
                                                                                                               .Text(ss => ss.Name("synonym")
                                                                                                                     .Analyzer("grade_level_index_synonym_analysis")
                                                                                                                     .SearchAnalyzer("grade_level_query_synonym_analysis")
                                                                                                                     .TermVector(TermVectorOption.WithPositionsOffsets))
                                                                                                               .Text(ss => ss.Name("base")
                                                                                                                     .Analyzer("grade_level_index_synonym_analysis")
                                                                                                                     .SearchAnalyzer("en_base_analysis")
                                                                                                                     .TermVector(TermVectorOption.WithPositionsOffsets))
                                                                                                               .Text(ss => ss.Name("nostop")
                                                                                                                     .Analyzer("grade_level_index_synonym_analysis")
                                                                                                                     .SearchAnalyzer("en_base_nostop_analysis")
                                                                                                                     .TermVector(TermVectorOption.WithPositionsOffsets))))
                                                            )
                                                )
                         )
                .Settings(x => x.Analysis(m => requestAnalysis));

            return client.CreateIndex(indexDescriptor);
        }

        #endregion
    }

    public static class Extensions
    {
        public static TextPropertyDescriptor<TsTemplate> multiFields(this TextPropertyDescriptor<TsTemplate> p)
        {
            return p.Fields(fs => fs
                    .Keyword(ss => ss.Name("literal"))
                    .Text(ss => ss.Name("base").Analyzer("en_base_analysis").TermVector(TermVectorOption.WithPositionsOffsets))
                    .Text(ss => ss.Name("nostop").Analyzer("en_base_nostop_analysis").TermVector(TermVectorOption.WithPositionsOffsets))
                    .Text(ss => ss.Name("ngram").Analyzer("en_ngram_analysis").TermVector(TermVectorOption.WithPositionsOffsets))
               );
        }
    }
}