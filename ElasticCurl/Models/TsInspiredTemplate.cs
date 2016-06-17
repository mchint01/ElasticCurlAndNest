using Nest;

namespace ElasticCurl.Models
{
    public class TsInspiredTemplate
    {
        [String(Name = "inspiredTemplateId",
            Index = FieldIndexOption.NotAnalyzed)]
        public string InspiredTemplateId { get; set; }

        [String(Name = "inspiredAuthor",
            Index = FieldIndexOption.Analyzed)]
        public string InspiredAuthor { get; set; }

        [String(Name = "inspiredAuthorAfmcCode",
            Index = FieldIndexOption.NotAnalyzed)]
        public string InspiredAuthorAfmcCode { get; set; }

        [String(Name = "inspiredAuthorProfileUrl",
            Index = FieldIndexOption.NotAnalyzed)]
        public string InspiredAuthorProfileUrl { get; set; }
    }
}