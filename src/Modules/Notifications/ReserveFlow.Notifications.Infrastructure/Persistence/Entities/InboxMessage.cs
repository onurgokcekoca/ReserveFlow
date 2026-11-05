namespace ReserveFlow.Notifications.Infrastructure.Persistence.Entities
{
    public sealed class InboxMessage
    {
        private InboxMessage()
        {
        }

        public Guid MessageId { get; private set; }
        public string MessageType { get; private set; } = default!;
        public DateTime ProcessedAtUtc { get; private set; }

        public static InboxMessage Create(
            Guid messageId,
            string messageType,
            DateTime processedAtUtc)
        {
            return new InboxMessage
            {
                MessageId = messageId,
                MessageType = messageType,
                ProcessedAtUtc = processedAtUtc
            };
        }
    }
}
