using Nest;
using Newtonsoft.Json;

namespace ElasticCurl.Models
{
    [ElasticsearchType(IdProperty = "Id", Name = "ts_suggestion")]
    public class TsSuggestion
    {
        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string Id { get; set; }

        [String(Name = "value",
            Index = FieldIndexOption.Analyzed)]
        public string Value { get; set; }

        [String(Name = "valueType",
            Index = FieldIndexOption.NotAnalyzed)]
        public string ValueType { get; set; }

        [String(Name = "imageUrl",
            Index = FieldIndexOption.NotAnalyzed)]
        public string ImageUrl { get; set; }

        [String(Name = "afmcCode",
            Index = FieldIndexOption.NotAnalyzed)]
        public string AfmcCode { get; set; }
    }
}
