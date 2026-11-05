using ReserveFlow.Catalog.Application.Properties;
using ReserveFlow.Catalog.Application.Properties.SearchProperties;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Application.Pagination;

internal sealed class SearchPropertiesQueryHandler : IQueryHandler<SearchPropertiesQuery, PagedResult<PropertyListItemResponse>>
{
    private readonly IPropertyReadRepository _propertyReadRepository;

    public SearchPropertiesQueryHandler(IPropertyReadRepository propertyReadRepository)
    {
        _propertyReadRepository = propertyReadRepository;
    }

    public async Task<PagedResult<PropertyListItemResponse>> HandleAsync(SearchPropertiesQuery query, CancellationToken cancellationToken)
    {
        int page = query.Page < 1 ? 1 : query.Page;

        int pageSize = query.PageSize switch
        {
            < 1 => 10,
            > 100 => 100,
            _ => query.PageSize
        };

        return await _propertyReadRepository.SearchAsync(
            query.Search,
            page,
            pageSize,
            cancellationToken);
    }
}
