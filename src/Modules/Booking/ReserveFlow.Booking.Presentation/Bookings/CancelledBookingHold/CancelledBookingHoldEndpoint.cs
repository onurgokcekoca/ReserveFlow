using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Booking.Application.Bookings.CancelledBookingHold;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Presentation.Extensions;

namespace ReserveFlow.Booking.Presentation.Bookings.CancelledBookingHold
{
    public static class CancelledBookingHoldEndpoint
    {
        public static void MapEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/booking/holds/{bookingId}/cancel", async (
                [FromRoute]
                Guid bookingId,
                [FromServices]
                ICommandHandler<CancelledBookingHoldCommand, AppResult> handler,
                CancellationToken cancellationToken

                ) =>
            {
                var command = new CancelledBookingHoldCommand(bookingId);

                var result = await handler.HandleAsync(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.NoContent();
                }

                return result.Error.ToProblem();
            }).WithTags("Booking").WithName("BookingCancel");
        }
    }
}
