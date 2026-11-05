using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Search.Application.AvailableRoomTypes;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Search.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSearchApplication(
            this IServiceCollection services)
        {
            services.AddScoped<IQueryHandler<SearchAvailableRoomTypesQuery, AppResult<SearchAvailableRoomTypesResponse>>, SearchAvailableRoomTypesQueryHandler>();

            return services;
        }
    }
}
