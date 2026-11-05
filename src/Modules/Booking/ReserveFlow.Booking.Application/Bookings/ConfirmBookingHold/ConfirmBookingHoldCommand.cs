using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Application.Bookings.ConfirmBookingHold;

public sealed record ConfirmBookingHoldCommand(Guid BookingHoldId) : ICommand<AppResult>;
