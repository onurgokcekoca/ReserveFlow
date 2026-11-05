using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Application.Bookings.ExpireBookingHold;

public sealed record ExpireBookingHoldCommand(Guid bookingId) : ICommand<AppResult>;
