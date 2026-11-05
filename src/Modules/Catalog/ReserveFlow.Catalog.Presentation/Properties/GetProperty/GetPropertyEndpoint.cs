using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Catalog.Application.Properties.GetProperty;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Presentation.Extensions;

namespace ReserveFlow.Catalog.Presentation.Properties.GetProperty;

internal static class GetPropertyEndpoint
{
    internal static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/properties/{propertyId:guid}",
            async (
                Guid propertyId,
                [FromServices]
                IQueryHandler<GetPropertyQuery, AppResult<PropertyDetailsResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetPropertyQuery(propertyId);

                AppResult<PropertyDetailsResponse> result = await handler.HandleAsync(query, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Ok(result.Value);
                }

                return result.Error.ToProblem();
            })
            .WithName("GetProperty")
            .WithTags("Properties");
    }
}
