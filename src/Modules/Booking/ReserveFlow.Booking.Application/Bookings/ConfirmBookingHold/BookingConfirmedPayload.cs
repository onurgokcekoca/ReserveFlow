namespace ReserveFlow.Booking.Application.Bookings.ConfirmBookingHold;

public sealed record BookingConfirmedPayload(Guid BookingId, Guid PropertyId, Guid RoomTypeId, DateOnly CheckIn, DateOnly CheckOut, int Rooms, int Guests);
