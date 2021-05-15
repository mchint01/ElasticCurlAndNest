using Newtonsoft.Json;

namespace Common.Elastic.Models
{
    public class TemplateTagIndex
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("tag_group_id")]
        public string TagGroupId { get; set; }

        [JsonProperty("tag_group_name")]
        public string TagGroupName { get; set; }

        [JsonProperty("tag_value")]
        public string TagValue { get; set; }
    }
}