using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Application.Bookings.CreateBookingHold;

public sealed record CreateBookingHoldCommand(Guid propertyId, Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms, int guests) : ICommand<AppResult<Guid>>;
