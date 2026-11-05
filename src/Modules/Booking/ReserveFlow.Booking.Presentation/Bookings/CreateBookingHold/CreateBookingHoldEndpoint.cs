using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Booking.Application.Bookings.CreateBookingHold;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Presentation.Extensions;

namespace ReserveFlow.Booking.Presentation.Bookings.CreateBookingHold
{
    public static class CreateBookingHoldEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/booking/holds", async (
                [FromBody]
                CreateBookingHoldRequest request,
                ICommandHandler<CreateBookingHoldCommand, AppResult<Guid>> handler,
                CancellationToken cancellationToken
                ) =>
            {
                var command = new CreateBookingHoldCommand(request.propertyId, request.roomTypeId, request.checkIn, request.checkOut, request.rooms, request.guests);

                var result = await handler.HandleAsync(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Created("$/booking/holds/{result.Value}", new { id = result.Value });
                }

                return result.Error.ToProblem();
            })
                .WithTags("Booking")
                .WithName("CreateBookingHold");
        }
    }
}
