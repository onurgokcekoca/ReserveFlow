namespace ReserveFlow.Search.Application.Common.Models;

public sealed record PropertySearchResult(
    IReadOnlyCollection<PropertySearchDocument> Items,
    long TotalCount,
    int Page,
    int PageSize,
    PropertySearchFacets Facets);
