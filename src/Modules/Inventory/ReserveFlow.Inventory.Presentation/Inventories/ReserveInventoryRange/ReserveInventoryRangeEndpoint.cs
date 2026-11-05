using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Inventory.Application.Inventories.ReserveInventoryRange;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Presentation.Extensions;

namespace ReserveFlow.Inventory.Presentation.Inventories.ReserveInventoryRange
{
    public static class ReserveInventoryRangeEndpoint
    {
        public static void MapEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/inventory/room-types/{roomTypeId}/reserve",
                async (
                    [FromRoute]
                    Guid roomTypeId,
                    ReserveInventoryRangeRequest request,
                    [FromServices]
                    ICommandHandler<ReserveInventoryRangeCommand, AppResult> handler,
                    CancellationToken cancellationToken) =>
            {
                var command = new ReserveInventoryRangeCommand(roomTypeId, request.CheckIn, request.CheckOut, request.Rooms);

                var result = await handler.HandleAsync(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.NoContent();
                }
                else
                {
                    return result.Error.ToProblem();
                }
            })
                .WithTags("Inventory")
                .WithName("ReserveInventoryRange");
        }
    }
}
