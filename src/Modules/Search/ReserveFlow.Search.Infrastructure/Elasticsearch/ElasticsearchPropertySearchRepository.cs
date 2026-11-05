using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;
using ReserveFlow.Search.Application.Common;
using ReserveFlow.Search.Application.Common.Models;

namespace ReserveFlow.Search.Infrastructure.Elasticsearch;

internal sealed class ElasticsearchPropertySearchRepository : IPropertySearchRepository
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;

    public ElasticsearchPropertySearchRepository(ElasticsearchClient client, IOptions<ElasticsearchOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task IndexAsync(PropertySearchDocument document, CancellationToken cancellationToken = default)
    {
        var response = await _client.IndexAsync(
            document,
            request => request
                .Index(_options.IndexName)
                .Id(document.PropertyId.ToString()),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to index property '{document.PropertyId}'. {response.DebugInformation}");
        }
    }

    public async Task<PropertySearchDocument?> GetAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync<PropertySearchDocument>(
            propertyId.ToString(),
            request => request.Index(_options.IndexName),
            cancellationToken);

        if (!response.Found)
        {
            return null;
        }

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to get property '{propertyId}'. {response.DebugInformation}");
        }

        return response.Source;
    }

    public async Task<PropertySearchResult> SearchAsync(PropertySearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var mustQueries = new List<Action<QueryDescriptor<PropertySearchDocument>>>();
        var filterQueries = new List<Action<QueryDescriptor<PropertySearchDocument>>>();

        if (!string.IsNullOrWhiteSpace(criteria.Query))
        {
            mustQueries.Add(q => q
                .MultiMatch(m => m
                    .Query(criteria.Query)
                    .Fields(Fields.FromStrings(["name", "description", "roomTypeNames"]))));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Country))
        {
            filterQueries.Add(q => q.Term(t => t.Field(x => x.Country).Value(criteria.Country)));
        }

        if (!string.IsNullOrWhiteSpace(criteria.City))
        {
            filterQueries.Add(q => q.Term(t => t.Field(x => x.City).Value(criteria.City)));
        }

        if (criteria.MinGuests.HasValue)
        {
            filterQueries.Add(q => q.Range(r => r.Number(n => n.Field(x => x.MaxGuests).Gte(criteria.MinGuests.Value))));
        }

        int from = (criteria.Page - 1) * criteria.PageSize;

        var response = await _client.SearchAsync<PropertySearchDocument>(
            search => search
                .Indices(_options.IndexName)
                .From(from)
                .Size(criteria.PageSize)
                .TrackTotalHits(true)
                .Query(q => q.Bool(b =>
                    {
                        if (mustQueries.Count > 0)
                        {
                            b.Must(mustQueries.ToArray());
                        }

                        if (filterQueries.Count > 0)
                        {
                            b.Filter(filterQueries.ToArray());
                        }
                    }))
                .Aggregations(aggregations => aggregations
                    .Add("cities", aggregation => aggregation
                        .Terms(terms => terms
                            .Field(x => x.City)
                            .Size(20)))
                    .Add("propertyTypes", aggregation => aggregation
                        .Terms(terms => terms
                            .Field(x => x.PropertyType)
                            .Size(20)))),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to search properties. {response.DebugInformation}");
        }

        long totalCount = response.HitsMetadata.Total?.Match(totalHits => totalHits?.Value ?? 0, value => value) ?? 0;

        StringTermsAggregate? citiesAggregation = response.Aggregations?.GetStringTerms("cities");

        StringTermsAggregate? propertyTypesAggregation = response.Aggregations?.GetStringTerms("propertyTypes");

        IReadOnlyCollection<PropertySearchFacetItem> cities = citiesAggregation?.Buckets.Select(bucket =>
                {
                    bucket.Key.TryGetString(out string? value);

                    return new PropertySearchFacetItem(value ?? string.Empty, bucket.DocCount);
                })
                .ToArray() ?? [];

        IReadOnlyCollection<PropertySearchFacetItem> propertyTypes = propertyTypesAggregation?.Buckets.Select(bucket =>
                {
                    bucket.Key.TryGetString(out string? value);

                    return new PropertySearchFacetItem(value ?? string.Empty, bucket.DocCount);
                })
                .ToArray() ?? [];

        var facets = new PropertySearchFacets(cities, propertyTypes);

        return new PropertySearchResult(response.Documents, totalCount, criteria.Page, criteria.PageSize, facets);
    }
}
