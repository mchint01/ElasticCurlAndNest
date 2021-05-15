namespace Common.Elastic.Interfaces
{
    public interface IConfigurationSettings
    {
        string RunningStack { get; }

        string ElasticBaseUrl { get; }

        string SuggestionIndexName { get; }

        string SearchIndexName { get; }
    }
}
