using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Elastic.Models;

namespace Common.Elastic.Interfaces
{
    public interface IJsonHelper
    {
        Task<string> GetJsonContentAsync(string fileName);

        Task<string> CreateBulkJsonContentAsync(List<SuggestionIndex> list);

        Task<string> CreateBulkJsonContentAsync(List<SearchIndex> list);
    }
}
