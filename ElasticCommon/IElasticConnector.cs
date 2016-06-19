using System.Threading.Tasks;
using ElasticCommon.Models;
using ElasticCommon.SearchModels;
using Nest;
using SearchRequest = ElasticCommon.Models.SearchRequest;

namespace ElasticCommon
{
    public interface IElasticConnector
    {
        ElasticClient GetClient();

        void IndexSuggestionDocument(IElasticClient client, TsSuggestion model);

        void DeleteSuggestionIndexAndReCreate(IElasticClient client);

        void OptimizeSuggestionIndex(IElasticClient client);

        Task<SearchResults<TsSuggestion>> GetSuggestions(IElasticClient client, SearchRequest request);

        void IndexTemplateDocument(IElasticClient client, TsTemplate model);

        void DeleteTemplateIndexAndReCreate(IElasticClient client);

        void OptimizeTemplateIndex(IElasticClient client);

        Task<SearchResults<TsTemplate>> GetTemplates(IElasticClient client, SearchRequest request);
    }
}