namespace ReserveFlow.Booking.Presentation.Bookings.CreateBookingHold;

public sealed record CreateBookingHoldRequest(Guid propertyId, Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms, int guests);
