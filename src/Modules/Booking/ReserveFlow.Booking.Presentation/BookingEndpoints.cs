using Microsoft.AspNetCore.Routing;
using ReserveFlow.Booking.Presentation.Bookings.CancelledBookingHold;
using ReserveFlow.Booking.Presentation.Bookings.ConfirmBookingHold;
using ReserveFlow.Booking.Presentation.Bookings.CreateBookingHold;

namespace ReserveFlow.Booking.Presentation
{
    public static class BookingEndpoints
    {
        public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
        {
            CreateBookingHoldEndpoint.MapEndpoint(app);
            ConfirmBookingHoldEndpoint.MapEndpoints(app);
            CancelledBookingHoldEndpoint.MapEndpoints(app);

            return app;
        }
    }
}
