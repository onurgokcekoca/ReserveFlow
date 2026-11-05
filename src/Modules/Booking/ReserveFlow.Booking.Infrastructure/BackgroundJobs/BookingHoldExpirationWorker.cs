using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReserveFlow.Booking.Application.Bookings;
using ReserveFlow.Booking.Application.Bookings.ExpireBookingHold;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

internal sealed partial class BookingHoldExpirationWorker(IServiceScopeFactory scopeFactory, ILogger<BookingHoldExpirationWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                IReadOnlyCollection<Guid> expiredBookingIds;

                using (var scope = scopeFactory.CreateScope())
                {
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                    expiredBookingIds = await bookingRepository.GetExpiredActiveHoldIdsAsync(DateTime.UtcNow, stoppingToken);
                }

                foreach (var bookingId in expiredBookingIds)
                {
                    try
                    {
                        using var scope = scopeFactory.CreateScope();

                        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExpireBookingHoldCommand, AppResult>>();

                        var result = await handler.HandleAsync(new ExpireBookingHoldCommand(bookingId), stoppingToken);

                        if (!result.IsSuccess)
                        {
                            LogBookingHoldCouldNotExpire(logger, bookingId, result.Error.Code);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogBookingHoldExpirationError(logger, ex, bookingId);
                    }
                }
            }
            catch (Exception ex)
            {
                LogExpiredBookingHoldSearchError(logger, ex);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Booking hold {HoldId} could not be expired. Error: {ErrorCode}")]
    private static partial void LogBookingHoldCouldNotExpire(ILogger logger, Guid holdId, string errorCode);

    [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while expiring booking hold {HoldId}.")]
    private static partial void LogBookingHoldExpirationError(ILogger logger, Exception exception, Guid holdId);

    [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while searching for expired booking holds.")]
    private static partial void LogExpiredBookingHoldSearchError(ILogger logger, Exception exception);
}
