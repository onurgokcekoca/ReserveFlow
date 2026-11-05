using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Inventory.Application.Abstractions;
using ReserveFlow.Inventory.Application.Inventories;
using ReserveFlow.Inventory.Application.Inventories.ReadServices;
using ReserveFlow.Inventory.Infrastructure.Persistence;
using ReserveFlow.Inventory.Infrastructure.Persistence.ReadServices;
using ReserveFlow.Inventory.Infrastructure.Persistence.Repositories;

namespace ReserveFlow.Inventory.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<InventoryDbContext>(options => options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "inventory")));

            services.AddScoped<IRoomInventoryRepository, RoomInventoryRepository>();
            services.AddScoped<IRoomInventoryReadRepository, RoomInventoryReadRepository>();
            services.AddScoped<IInventoryUnitOfWork>(provider => provider.GetRequiredService<InventoryDbContext>());
            services.AddScoped<IRoomAvailabilityReadService, RoomAvailabilityReadService>();

            return services;
        }
    }
}
