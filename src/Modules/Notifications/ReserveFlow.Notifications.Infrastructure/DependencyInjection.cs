using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Notifications.Infrastructure.Messaging;
using ReserveFlow.Notifications.Infrastructure.Persistence;

namespace ReserveFlow.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.AddDbContext<NotificationsDbContext>(options => options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "notifications")));

        services.Configure<NotificationsMessagingOptions>(configuration.GetSection(NotificationsMessagingOptions.SectionName));

        services.AddSingleton<BookingConfirmedTopology>();

        services.AddHostedService<BookingConfirmedConsumerWorker>();

        return services;
    }

    public static async Task InitializeNotificationInfrastructureAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        BookingConfirmedTopology topology = serviceProvider.GetRequiredService<BookingConfirmedTopology>();

        await topology.EnsureAsync(cancellationToken);
    }
}
