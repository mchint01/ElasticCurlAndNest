using System.Collections.Generic;

namespace Common.Elastic.Models
{
    public class SuggestionIndexResponse
    {
        public long TotalRecords { get; set; }

        public List<SuggestionIndex> Data { get; set; }
    }
}
