using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Booking.Application.Bookings.CancelledBookingHold;
using ReserveFlow.Booking.Application.Bookings.ConfirmBookingHold;
using ReserveFlow.Booking.Application.Bookings.CreateBookingHold;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBookingApplication(this IServiceCollection services)
        {
            services.AddScoped<ICommandHandler<CreateBookingHoldCommand, AppResult<Guid>>, CreateBookingHoldCommandHandler>();

            services.AddScoped<ICommandHandler<ConfirmBookingHoldCommand, AppResult>, ConfirmBookingHoldCommandHandler>();

            services.AddScoped<ICommandHandler<CancelledBookingHoldCommand, AppResult>, CancelledBookingHoldCommandHandler>();

            return services;
        }
    }
}
