using System.Collections.Generic;

namespace Common.Elastic.Models
{
    public class SearchIndexRequest
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string EscapedTitle { get; set; }

        public string Description { get; set; }

        public string TemplateUri { get; set; }

        public string TemplateAuthorName { get; set; }

        public string TemplateAuthorId { get; set; }

        public string TemplateCode { get; set; }

        public string TemplateAuthorSchoolDistrict { get; set; }

        public string TemplateAuthorAfmcCode { get; set; }

        public bool TemplateIsFeatured { get; set; }

        public bool TemplateIsDeleted { get; set; }

        public bool TemplateIsUploaded { get; set; }

        public bool TemplateIsArticle { get; set; }

        public bool TemplateIsLandscape { get; set; }

        public string TemplateInspiredTemplateId { get; set; }

        public string TemplateInspiredAuthorName { get; set; }

        public string TemplateInspiredTemplateAfmcCode { get; set; }

        public string TemplateSpellData { get; set; }

        public string TemplateInspiredAuthorUri { get; set; }

        public string TemplateInspiredAuthorAfmcCode { get; set; }

        public string ModifiedBy { get; set; }

        public int TemplateDownloadCount { get; set; }

        public int TemplateClonedCount { get; set; }

        public int TemplateSmileyCount { get; set; }

        public List<TemplateTagIndexRequest> TemplateTags { get; set; }
    }
}
