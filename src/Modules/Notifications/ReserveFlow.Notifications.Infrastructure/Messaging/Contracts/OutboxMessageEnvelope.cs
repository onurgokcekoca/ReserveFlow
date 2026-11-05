using System.Text.Json;

namespace ReserveFlow.Notifications.Infrastructure.Messaging.Contracts;

public sealed record OutboxMessageEnvelope(
    Guid MessageId,
    string Type,
    DateTime OccurredAtUtc,
    JsonElement Payload);
