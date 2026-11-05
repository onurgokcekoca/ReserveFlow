namespace ReserveFlow.Notifications.Infrastructure.Persistence.Entities
{
    public sealed class BookingConfirmationNotification
    {
        private BookingConfirmationNotification()
        {
        }

        public Guid Id { get; private set; }
        public Guid MessageId { get; private set; }
        public Guid BookingId { get; private set; }
        public Guid PropertyId { get; private set; }
        public DateTime OccurredAtUtc { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        public static BookingConfirmationNotification Create(
            Guid messageId,
            Guid bookingId,
            Guid propertyId,
            DateTime occurredAtUtc)
        {
            return new BookingConfirmationNotification
            {
                Id = Guid.CreateVersion7(),
                MessageId = messageId,
                BookingId = bookingId,
                PropertyId = propertyId,
                OccurredAtUtc = occurredAtUtc,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
    }
}
