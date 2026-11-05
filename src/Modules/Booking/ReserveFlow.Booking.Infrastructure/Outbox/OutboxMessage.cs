namespace ReserveFlow.Booking.Infrastructure.Outbox
{
    public sealed class OutboxMessage
    {
        private OutboxMessage()
        {
        }

        private OutboxMessage(Guid id, string type, string payload, DateTime occurredAtUtc)
        {
            Id = id;
            Type = type;
            Payload = payload;
            OccurredAtUtc = occurredAtUtc;
        }

        public Guid Id { get; private set; }
        public string Type { get; private set; } = default!;
        public string Payload { get; private set; } = default!;
        public DateTime OccurredAtUtc { get; private set; }
        public DateTime? ProcessedAtUtc { get; private set; }

        public static OutboxMessage Create(string type, string payload, DateTime occurredAtUtc)
        {
            return new OutboxMessage(Guid.CreateVersion7(), type, payload, occurredAtUtc);
        }

        public void MarkAsProcessed(DateTime processedAtUtc)
        {
            if (ProcessedAtUtc is not null)
            {
                return;
            }

            ProcessedAtUtc = processedAtUtc;
        }
    }
}
