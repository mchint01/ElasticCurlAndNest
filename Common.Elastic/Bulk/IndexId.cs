using Newtonsoft.Json;

namespace Common.Elastic.Bulk
{
    public class Index
    {
        public Index(string id)
        {
            Id = new IndexId(id);
        }

        [JsonProperty("index")]
        public IndexId Id { get; set; }
    }

    public class IndexId
    {
        public IndexId(string id)
        {
            Id = id;
        }

        [JsonProperty("_id")]
        public string Id { get; set; }
    }
}
