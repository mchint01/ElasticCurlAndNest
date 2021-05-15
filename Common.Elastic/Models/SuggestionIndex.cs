using Newtonsoft.Json;

namespace Common.Elastic.Models
{
    public class SuggestionIndex
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the type of the value.
        /// values are: Template, Author, Tag, TagGroup
        /// </summary>
        /// <value>
        /// The type of the value.
        /// </value>
        [JsonProperty("value_type")]
        public string ValueType { get; set; }

        [JsonProperty("image_uri")]
        public string ImageUri { get; set; }

        [JsonProperty("afmc_code")]
        public string AfmcCode { get; set; }

        [JsonProperty("tmpl_author")]
        public string TemplateAuthorName { get; set; }

        [JsonProperty("tmpl_author_role")]
        public string TemplateAuthorRole { get; set; }

        [JsonProperty("tmpl_esc_value")]
        public string TemplateName { get; set; }

        [JsonProperty("tmpl_is_deleted")]
        public bool TemplateIsDeleted { get; set; }

        [JsonProperty("tmpl_is_uploaded")]
        public bool TemplateIsUploaded { get; set; }

        [JsonProperty("tmpl_is_article")]
        public bool TemplateIsArticle{ get; set; }

        [JsonProperty("tmpl_is_landscape")]
        public bool TemplateIsLandscape { get; set; }
    }
}
