namespace ElasticCommon.Models
{
    public class SearchRequest
    {
        public int PageSize { get; set; }

        public int CurrentPage { get; set; }

        public double MinScore { get; set; }

        public string Query { get; set; }

        public string Filter { get; set; }
    }
}