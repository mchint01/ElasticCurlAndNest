using ElasticCommon.Converter;
using Nest;
using Newtonsoft.Json;

namespace ElasticCommon.SearchModels
{
    [ElasticsearchType(IdProperty = "id", Name = "ts_suggestion")]
    public class TsSuggestion
    {
        [String(Index = FieldIndexOption.NotAnalyzed,
            Name = "id")]
        public string Id { get; set; }

        [String(Name = "value",
            Index = FieldIndexOption.Analyzed,
            Analyzer = "suggestionAnalyzer")]
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

        [String(Name = "tmplAuthorName",
            Index = FieldIndexOption.NotAnalyzed)]
        public string TmplAuthorName { get; set; }

        [String(Name = "escTitle",
            Index = FieldIndexOption.NotAnalyzed)]
        public string EscTitle { get; set; }

        [Boolean(Name = "deleted",
            Index = NonStringIndexOption.NotAnalyzed)]
        [JsonConverter(typeof(BoolConverter))]
        public bool Deleted { get; set; }

        public double Score { get; set; }

        [Boolean(Name = "isUploaded",
            Index = NonStringIndexOption.NotAnalyzed)]
        public bool IsUploaded { get; set; }

        [Boolean(Name = "isLandscape ",
            Index = NonStringIndexOption.NotAnalyzed)]
        public bool IsLandscape { get; set; }
    }
}
