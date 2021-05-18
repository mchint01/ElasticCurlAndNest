using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Elastic.Models;

namespace Common.Elastic.Interfaces
{
    public interface IIndexClient
    {
        Task PingAsync();

        Task RegisterIndicesAsync();

        Task SeedSuggestionIndexAsync(SuggestionIndexRequest request);

        Task SeedSuggestionsIndexAsync(List<SuggestionIndexRequest> requests);

        Task SeedSearchIndexAsync(SearchIndexRequest request);

        Task SeedSearchesIndexAsync(List<SearchIndexRequest> requests);
    }
}