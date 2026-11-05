using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Catalog.Application.Abstractions;
using ReserveFlow.Catalog.Application.Abstractions.Caching;
using ReserveFlow.Catalog.Application.Properties;
using ReserveFlow.Catalog.Application.Properties.ReadServices;
using ReserveFlow.Catalog.Infrastructure.Caching;
using ReserveFlow.Catalog.Infrastructure.Messaging;
using ReserveFlow.Catalog.Infrastructure.Outbox;
using ReserveFlow.Catalog.Infrastructure.Persistence;
using ReserveFlow.Catalog.Infrastructure.Persistence.ReadServices;
using ReserveFlow.Catalog.Infrastructure.Persistence.Repositories;

namespace ReserveFlow.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "catalog")));

        services.AddScoped<IPropertyRepository, PropertyRepository>();

        services.AddScoped<ICatalogUnitOfWork>(provider => provider.GetRequiredService<CatalogDbContext>());

        services.AddScoped<IPropertyReadRepository, PropertyReadRepository>();

        services.AddScoped<IPropertyRoomTypeReadService, PropertyRoomTypeReadService>();

        services.AddScoped<IOutboxWriter, OutboxWriter>();

        bool redisEnabled = bool.TryParse(configuration["Redis:Enabled"], out bool enabled) && enabled;

        if (redisEnabled)
        {
            string redisConnectionString = configuration["Redis:ConnectionString"] ?? throw new InvalidOperationException("Redis connection string is missing.");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });

            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, NoOpCacheService>();
        }

        services.AddSingleton<CatalogMessagingInitializer>();

        services.AddScoped<RabbitMqOutboxPublisher>();

        services.AddHostedService<OutboxPublisherWorker>();

        return services;
    }

    public static async Task InitializeCatalogInfrastructureAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        CatalogMessagingInitializer initializer = serviceProvider.GetRequiredService<CatalogMessagingInitializer>();

        await initializer.InitializeAsync(cancellationToken);
    }
}
