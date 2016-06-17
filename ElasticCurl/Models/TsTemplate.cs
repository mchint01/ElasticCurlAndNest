using Nest;
using System;
using System.Collections.Generic;

namespace ElasticCurl.Models
{
    public class TsTemplate
    {
        public TsTemplate()
        {
            this.Tags = new List<string>();
            this.TemplateTypes = new List<string>();
        }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public string TemplateId { get; set; }

        [String(Name = "templateTitle",
            Index = FieldIndexOption.Analyzed)]
        public string TemplateTitle { get; set; }

        [String(Name = "templateDescription",
            Index = FieldIndexOption.Analyzed)]
        public string TemplateDescription { get; set; }

        [String(Name = "templateThumbnailUri",
            Index = FieldIndexOption.NotAnalyzed)]
        public string TemplateThumbnailUri { get; set; }

        [String(Name = "author",
            Index = FieldIndexOption.Analyzed)]
        public string Author { get; set; }

        [String(Name = "authorThumbnailUri",
            Index = FieldIndexOption.NotAnalyzed)]
        public string AuthorThumbnailUri { get; set; }

        [String(Name = "schoolDistrict",
            Index = FieldIndexOption.Analyzed)]
        public string SchoolDisctrict { get; set; }

        [String(Name = "authorAfmcCode",
            Index = FieldIndexOption.NotAnalyzed)]
        public string AuthorAfmcCode { get; set; }

        [Nested(Name = "tags")]
        public IList<string> Tags { get; set; }

        [Nested(Name = "templateTypes")]
        public IList<string> TemplateTypes { get; set; }

        [String(Name = "authorId",
            Index = FieldIndexOption.NotAnalyzed)]
        public string AuthorId { get; set; }

        [String(Name = "templateCode",
            Index = FieldIndexOption.Analyzed)]
        public string TemplateCode { get; set; }

        [String(Name = "templateAttributeFeatured",
            Index = FieldIndexOption.NotAnalyzed)]
        public string TemplateAttributeFeatured { get; set; }


        [Nested(Name = "inspiredFrom")]
        public TsInspiredTemplate InspiredFrom { get; set; }

        public double Score { get; set; }
    }
}