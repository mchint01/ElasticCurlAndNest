namespace Common.Elastic.Models
{
    public class TemplateTagIndexRequest
    {
        public string Id { get; set; }

        public string TagGroupId { get; set; }

        public string TagGroupName { get; set; }

        public string TagValue { get; set; }
    }
}