using System;

namespace Common.Elastic.Interfaces
{
    public interface IIndexSettings
    {
        Uri GetClusterHealthUri();

        Uri GetSuggestionIndexUri();

        Uri GetSearchIndexUri();

        Uri GetSuggestionIndexUri(string documentId);

        Uri GetSearchIndexUri(string documentId);

        Uri GetSuggestionIndexForBulkIndexUri();

        Uri GetSearchIndexForBulkIndexUri();
    }
}
