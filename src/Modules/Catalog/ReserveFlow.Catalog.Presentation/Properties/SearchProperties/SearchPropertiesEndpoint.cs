using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Catalog.Application.Properties.SearchProperties;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Application.Pagination;

namespace ReserveFlow.Catalog.Presentation.Properties.SearchProperties;

internal static class SearchPropertiesEndpoint
{
    internal static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/properties",
            async (
                string? search,
                int? page,
                int? pageSize,
                [FromServices]
                IQueryHandler<  SearchPropertiesQuery,PagedResult<PropertyListItemResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new SearchPropertiesQuery(
                    search,
                    page ?? 1,
                    pageSize ?? 10);

                PagedResult<PropertyListItemResponse> result = await handler.HandleAsync(query, cancellationToken);

                return Results.Ok(result);
            })
            .WithName("SearchProperties")
            .WithTags("Properties");
    }
}
