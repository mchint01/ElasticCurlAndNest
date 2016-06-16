namespace ElasticCurl.Models
{
    public class SuggestionRequest
    {
        public int PageSize { get; set; }

        public int CurrentPage { get; set; }

        public double MinScore { get; set; }

        public string Query { get; set; }
    }
}