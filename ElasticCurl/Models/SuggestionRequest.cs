namespace ElasticCurl.Models
{
    public class SuggestionRequest
    {
        public int Skip { get; set; }

        public int Top { get; set; }

        public string Query { get; set; }
    }
}