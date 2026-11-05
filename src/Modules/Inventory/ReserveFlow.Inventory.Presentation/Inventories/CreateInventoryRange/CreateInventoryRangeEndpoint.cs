using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Inventory.Application.Inventories.CreateInventoryRange;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Presentation.Extensions;

namespace ReserveFlow.Inventory.Presentation.Inventories.Availability
{
    internal static class CreateInventoryRangeEndpoint
    {
        internal static void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/inventory/room-types/{roomTypeId}/availability",
                async (
                    Guid roomTypeId,
                    CreateInventoryRangeRequest request,
                    ICommandHandler<CreateInventoryRangeCommand, AppResult<Guid>> handler,
                    CancellationToken cancellationToken) =>
                {
                    var command = new CreateInventoryRangeCommand(
                        roomTypeId,
                        request.StartDate,
                        request.EndDate,
                        request.TotalRooms
                        );

                    AppResult<Guid> result =
                        await handler.HandleAsync(command, cancellationToken);

                    if (result.IsSuccess)
                    {
                        return Results.Created($"/availability/{result.Value}", new { Id = result.Value });
                    }

                    return result.Error.ToProblem();
                })
                .WithName("CreateInventoryRange")
                .WithTags("Inventory");
        }
    }
}
