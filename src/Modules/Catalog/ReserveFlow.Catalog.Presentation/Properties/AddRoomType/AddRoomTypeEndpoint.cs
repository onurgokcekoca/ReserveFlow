using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Catalog.Application.Properties.AddRoomType;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Presentation.Extensions;

namespace ReserveFlow.Catalog.Presentation.Properties.AddRoomType;

internal static class AddRoomTypeEndpoint
{
    internal static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/properties/{propertyId:guid}/room-types",
            async (
                Guid propertyId,
                AddRoomTypeRequest request,
                [FromServices]
                ICommandHandler<AddRoomTypeCommand, AppResult<Guid>> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new AddRoomTypeCommand(
                    propertyId,
                    request.Name,
                    request.Capacity,
                    request.TotalRooms,
                    request.BasePriceAmount,
                    request.Currency);

                AppResult<Guid> result = await handler.HandleAsync(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Created(
                        $"/properties/{propertyId}/room-types/{result.Value}",
                        new { Id = result.Value });
                }

                return result.Error.ToProblem();
            })
            .WithName("AddRoomType")
            .WithTags("Properties");
    }
}
