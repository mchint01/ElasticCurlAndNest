using System.Collections.Generic;

namespace Common.Elastic.Models
{
    public class FilterSearchIndexRequest
    {
        public string SortBy { get; set; }

        public string SortByDirection { get; set; }

        public string SearchData { get; set; }

        public List<FilterTagRequest> FilterTagRequests { get; set; }
    }
}
