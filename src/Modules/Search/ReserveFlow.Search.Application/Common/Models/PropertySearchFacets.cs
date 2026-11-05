namespace ReserveFlow.Search.Application.Common.Models;

public sealed record PropertySearchFacets(
    IReadOnlyCollection<PropertySearchFacetItem> Cities,
    IReadOnlyCollection<PropertySearchFacetItem> PropertyTypes);
