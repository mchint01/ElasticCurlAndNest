namespace Common.Elastic.Interfaces
{
    public interface IConfigurationSettings
    {
        string ElasticBaseUrl { get; }

        string SuggestionIndexName { get; }

        string SearchIndexName { get; }
    }
}
