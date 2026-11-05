using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Application.Bookings.CancelledBookingHold;

public sealed record CancelledBookingHoldCommand(Guid bookingId) : ICommand<AppResult>;
