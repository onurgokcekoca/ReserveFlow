using System.Text.Json;

namespace ReserveFlow.Shared.Application.Messaging;

public sealed record IntegrationEventEnvelope(
    Guid MessageId,
    string Type,
    DateTime OccurredAtUtc,
    JsonElement Payload);
