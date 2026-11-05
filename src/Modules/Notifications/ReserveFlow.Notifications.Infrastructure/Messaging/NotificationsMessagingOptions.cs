namespace ReserveFlow.Notifications.Infrastructure.Messaging;

public sealed class NotificationsMessagingOptions
{
    public const string SectionName = "NotificationsMessaging";

    public string BookingConfirmedQueueName { get; init; } = "booking-confirmed";

    public string BookingConfirmedRetryQueueName { get; init; } = "booking-confirmed-retry";

    public string BookingConfirmedDeadLetterQueueName { get; init; } = "booking-confirmed-dead-letter";

    public int RetryDelayMilliseconds { get; init; } = 15_000;

    public int MaxRetryCount { get; init; } = 3;
}
