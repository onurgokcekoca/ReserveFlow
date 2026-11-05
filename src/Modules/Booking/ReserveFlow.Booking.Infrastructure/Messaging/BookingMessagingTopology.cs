namespace ReserveFlow.Booking.Infrastructure.Messaging;

internal static class BookingMessagingTopology
{
    public const string ExchangeName = "booking.events";

    public const string BookingConfirmedEventType = "BookingConfirmed";

    public const string BookingConfirmedRoutingKey = "booking.confirmed";
}
