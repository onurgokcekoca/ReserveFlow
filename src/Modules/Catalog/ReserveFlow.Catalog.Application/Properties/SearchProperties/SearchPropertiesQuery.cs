using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Application.Pagination;

namespace ReserveFlow.Catalog.Application.Properties.SearchProperties;

public sealed record SearchPropertiesQuery(string? Search, int Page, int PageSize) : IQuery<PagedResult<PropertyListItemResponse>>;
