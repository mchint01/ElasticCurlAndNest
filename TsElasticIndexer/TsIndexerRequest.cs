using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsElasticIndexer
{
    public class TsIndexerRequest
    {
        public string DocumentDbEndpointUrl { get; set; }

        public string AuthorizationKey { get; set; }

        public string DatabaseId { get; set; }

        public string SuggestionCollectionId { get; set; }

        public string TemplateCollectionId { get; set; }

        public string[] ElasticClusterUris { get; set; }

        public string ElasticAdminUserName { get; set; }

        public string ElasticAdminPassword { get; set; }
    }
}
