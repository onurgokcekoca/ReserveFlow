using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Inventory.Application.Inventories.ReleaseInventoryRange;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Presentation.Extensions;

namespace ReserveFlow.Inventory.Presentation.Inventories.ReleaseInventoryRange
{
    public static class ReleaseInventoryRangeEndpoint
    {
        public static void MapEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/inventory/room-types/{roomTypeId}/release",
                async (
                    [FromRoute]
                    Guid roomTypeId,
                    [FromBody]
                    ReleaseInventoryRangeRequest request,
                    [FromServices]
                    ICommandHandler<ReleaseInventoryRangeCommand, AppResult> handler,
                    CancellationToken cancellationToken) =>
            {
                var query = new ReleaseInventoryRangeCommand(roomTypeId, request.checkIn, request.checkOut, request.rooms);

                var result = await handler.HandleAsync(query, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.NoContent();
                }

                return result.Error.ToProblem();
            })
                .WithTags("Inventory")
                .WithName("ReleaseInventoryRange");
        }
    }
}
