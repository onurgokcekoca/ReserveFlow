using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Booking.Presentation.Exceptions;

namespace ReserveFlow.Booking.Presentation
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBookingPresentation(this IServiceCollection services)
        {
            services.AddExceptionHandler<BookingHoldConcurrencyExceptionHandler>();

            return services;
        }
    }
}
