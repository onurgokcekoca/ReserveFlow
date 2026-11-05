using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Booking.Application.Abstractions;
using ReserveFlow.Booking.Application.Bookings;
using ReserveFlow.Booking.Application.Bookings.ExpireBookingHold;
using ReserveFlow.Booking.Infrastructure.Messaging;
using ReserveFlow.Booking.Infrastructure.Outbox;
using ReserveFlow.Booking.Infrastructure.Persistence;
using ReserveFlow.Booking.Infrastructure.Persistence.Gateways;
using ReserveFlow.Booking.Infrastructure.Persistence.Repositories;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BookingDbContext>(options => options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "booking")));

        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddScoped<IOutboxWriter, OutboxWriter>();

        services.AddScoped<IBookingUnitOfWork>(provider => provider.GetRequiredService<BookingDbContext>());

        services.AddScoped<IInventoryReservationGateway, InventoryReservationGateway>();

        services.AddScoped<ICommandHandler<ExpireBookingHoldCommand, AppResult>, ExpireBookingHoldCommandHandler>();

        services.AddSingleton<RabbitMqOutboxPublisher>();

        services.AddSingleton<BookingMessagingInitializer>();

        services.AddHostedService<BookingHoldExpirationWorker>();

        services.AddHostedService<OutboxPublisherWorker>();

        return services;
    }

    public static async Task InitializeBookingInfrastructureAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        BookingMessagingInitializer initializer = serviceProvider.GetRequiredService<BookingMessagingInitializer>();

        await initializer.InitializeAsync(cancellationToken);
    }
}
