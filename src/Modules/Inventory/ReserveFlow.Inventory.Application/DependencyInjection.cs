using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Inventory.Application.Inventories.CreateInventoryRange;
using ReserveFlow.Inventory.Application.Inventories.GetRoomAvailability;
using ReserveFlow.Inventory.Application.Inventories.ReleaseInventoryRange;
using ReserveFlow.Inventory.Application.Inventories.ReserveInventoryRange;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Inventory.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInventoryApplication(this IServiceCollection services)
        {
            services.AddScoped<ICommandHandler<CreateInventoryRangeCommand, AppResult<Guid>>, CreateInventoryRangeCommandHandler>();

            services.AddScoped<IQueryHandler<GetRoomAvailabilityQuery, AppResult<RoomAvailabilityResponse>>, GetRoomAvailabilityQueryHandler>();

            services.AddScoped<ICommandHandler<ReserveInventoryRangeCommand, AppResult>, ReserveInventoryRangeCommandHandler>();

            services.AddScoped<ICommandHandler<ReleaseInventoryRangeCommand, AppResult>, ReleaseInventoryRangeCommandHandler>();

            return services;
        }
    }
}
