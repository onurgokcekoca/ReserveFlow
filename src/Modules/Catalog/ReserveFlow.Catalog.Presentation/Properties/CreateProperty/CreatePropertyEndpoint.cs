using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Catalog.Application.Properties.CreateProperty;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Presentation.Extensions;

namespace ReserveFlow.Catalog.Presentation.Properties.CreateProperty;

internal static class CreatePropertyEndpoint
{
    internal static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/properties",
            async (
                CreatePropertyRequest request,
                ICommandHandler<CreatePropertyCommand, AppResult<Guid>> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreatePropertyCommand(
                    request.Name,
                    request.Description,
                    request.Country,
                    request.City,
                    request.AddressLine,
                    request.Latitude,
                    request.Longitude,
                    request.PropertyType);

                AppResult<Guid> result = await handler.HandleAsync(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Created(
                        $"/properties/{result.Value}",
                        new { Id = result.Value });
                }

                return result.Error.ToProblem();
            })
            .WithName("CreateProperty")
            .WithTags("Properties");
    }
}
