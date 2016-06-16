using System.Collections.Generic;

namespace ElasticCurl.Models
{
    public class SearchResults<T>
    {
        public IEnumerable<T> Results { get; set; }
        public long Count { get; set; }
        public string Query { get; set; }
        public long Ticks { get; set; }
    }
}