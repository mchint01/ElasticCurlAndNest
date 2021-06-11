using System.Collections.Generic;

namespace Common.Elastic.Models
{
    public class SearchIndexResponse
    {
        public long TotalRecords { get; set; }

        public List<SearchIndex> Data { get; set; }
    }
}
