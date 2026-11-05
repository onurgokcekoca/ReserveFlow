namespace ReserveFlow.Search.Infrastructure.Elasticsearch;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Url { get; init; } = string.Empty;
    public string IndexName { get; init; } = string.Empty;
}
