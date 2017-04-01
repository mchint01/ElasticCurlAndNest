using ElasticCommon.Converter;
using Nest;
using Newtonsoft.Json;

namespace ElasticCommon.SearchModels
{
    [ElasticsearchType(IdProperty = "id", Name = "ts_suggestion")]
    public class TsSuggestion
    {
        [Keyword(Name = "id")]
        public string Id { get; set; }

        [Text(Name = "value",
            Analyzer = "suggestionAnalyzer")]
        public string Value { get; set; }

        [Keyword(Name = "valueType")]
        public string ValueType { get; set; }

        [Keyword(Name = "imageUrl")]
        public string ImageUrl { get; set; }

        [Keyword(Name = "afmcCode")]
        public string AfmcCode { get; set; }

        [Keyword(Name = "tmplAuthorName")]
        public string TmplAuthorName { get; set; }

        [Keyword(Name = "escTitle")]
        public string EscTitle { get; set; }

        [Boolean(Name = "deleted")]
        [JsonConverter(typeof(BoolConverter))]
        public bool Deleted { get; set; }

        public double Score { get; set; }

        [Boolean(Name = "isUploaded")]
        public bool IsUploaded { get; set; }

        [Boolean(Name = "isLandscape ")]
        public bool IsLandscape { get; set; }
    }
}