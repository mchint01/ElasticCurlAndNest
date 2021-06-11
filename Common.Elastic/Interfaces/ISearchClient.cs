using System.Threading.Tasks;
using Common.Elastic.Models;

namespace Common.Elastic.Interfaces
{
    public interface ISearchClient
    {
        // search in search index
        Task<SearchIndexResponse> SearchIndexAsync(
            FilterSearchIndexRequest filterSearchIndexRequest);

        // search in suggestion index
        Task<SuggestionIndexResponse> SearchSuggestionIndexAsync(
            FilterSuggestionIndexRequest filterSuggestionIndexRequest);
    }
}
