using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Common.Elastic.Models
{
    public class SuggestionIndex
    {
        [JsonProperty("document_type")]
        public string Id { get; set; }

        public string Value { get; set; }

        public string ValueType { get; set; }

        public string ImageUri { get; set; }

        public string AfmcCode { get; set; }

        public string TemplateAuthorName { get; set; }

        public string TemplateAuthorRole { get; set; }

        public string TemplateName { get; set; }

        public bool TemplateIsDeleted { get; set; }

        public bool TemplateIsUploaded { get; set; }

        public bool TemplateIsArticle{ get; set; }

        public bool TemplateIsLandscape { get; set; }
    }
}
