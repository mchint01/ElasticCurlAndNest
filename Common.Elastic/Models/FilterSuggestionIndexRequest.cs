namespace Common.Elastic.Models
{
    public class FilterSuggestionIndexRequest
    {
        public string SortBy { get; set; }

        public string SortByDirection { get; set; }

        public string SearchData { get; set; }
    }
}