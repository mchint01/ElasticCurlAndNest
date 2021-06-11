using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Elastic.Interfaces;
using Common.Elastic.Models;
using Nest;

namespace Common.Elastic.Clients
{
    public class SearchClient : ISearchClient
    {
        private readonly IElasticClient _elasticClient;

        private readonly IConfigurationSettings _configurationSettings;

        private const int MaxSearchResultSize = 10000;

        public SearchClient(
            IElasticClient elasticClient,
            IConfigurationSettings configurationSettings)
        {
            _elasticClient = elasticClient;

            _configurationSettings = configurationSettings;
        }

        public Task<SearchIndexResponse> SearchIndexAsync(
            FilterSearchIndexRequest filterSearchIndexRequest)
        {
            throw new NotImplementedException();
        }

        public Task<SuggestionIndexResponse> SearchSuggestionIndexAsync(
            FilterSuggestionIndexRequest filterSuggestionIndexRequest)
        {
            throw new NotImplementedException();
        }
    }
}
