using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Elastic.Models
{
    public class SearchIndex
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("esc_title")]
        public string EscapedTitle { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("tmpl_uri")]
        public string TemplateUri { get; set; }

        [JsonProperty("tmpl_author")]
        public string TemplateAuthorName { get; set; }

        [JsonProperty("tmpl_author_id")]
        public string TemplateAuthorId { get; set; }

        [JsonProperty("tmpl_code")]
        public string TemplateCode { get; set; }

        [JsonProperty("tmpl_author_school_district")]
        public string TemplateAuthorSchoolDistrict { get; set; }

        [JsonProperty("tmpl_author_afmc_code")]
        public string TemplateAuthorAfmcCode { get; set; }

        [JsonProperty("tmpl_is_featured")]
        public bool TemplateIsFeatured { get; set; }

        [JsonProperty("tmpl_is_deleted")]
        public bool TemplateIsDeleted { get; set; }

        [JsonProperty("tmpl_is_uploaded")]
        public bool TemplateIsUploaded { get; set; }

        [JsonProperty("tmpl_is_article")]
        public bool TemplateIsArticle { get; set; }

        [JsonProperty("tmpl_is_landscape")]
        public bool TemplateIsLandscape { get; set; }

        [JsonProperty("tmpl_inspired_tmpl_id")]
        public string TemplateInspiredTemplateId { get; set; }

        [JsonProperty("tmpl_inspired_author_name")]
        public string TemplateInspiredAuthorName { get; set; }

        [JsonProperty("tmpl_inspired_tmpl_afmc_code")]
        public string TemplateInspiredTemplateAfmcCode { get; set; }

        [JsonProperty("tmpl_spell_data")]
        public string TemplateSpellData { get; set; }

        [JsonProperty("tmpl_author_uri")]
        public string TemplateAuthorUri { get; set; }

        [JsonProperty("tmpl_inspired_author_uri")]
        public string TemplateInspiredAuthorUri { get; set; }

        [JsonProperty("tmpl_inspired_author_afmc_code")]
        public string TemplateInspiredAuthorAfmcCode { get; set; }

        [JsonProperty("modified_by")]
        public string ModifiedBy { get; set; }

        [JsonProperty("tmpl_download_count")]
        public int TemplateDownloadCount { get; set; }

        [JsonProperty("tmpl_cloned_count")]
        public int TemplateClonedCount { get; set; }

        [JsonProperty("tmpl_smiley_count")]
        public int TemplateSmileyCount { get; set; }

        [JsonProperty("tmpl_tags")]
        public List<TemplateTagIndex> TemplateTags { get; set; }
    }
}
