namespace ReserveFlow.Search.Infrastructure.Messaging
{
    internal sealed class SearchMessagingOptions
    {
        public const string SectionName = "SearchMessaging";
        public int MaxRetryCount { get; init; } = 3;
        public int RetryDelayMilliseconds { get; init; } = 15000;
    }
}
