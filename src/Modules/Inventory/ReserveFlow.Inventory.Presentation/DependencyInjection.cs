using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Inventory.Presentation.Exceptions;

namespace ReserveFlow.Inventory.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryPresentation(
        this IServiceCollection services)
    {
        services.AddExceptionHandler<InventoryConcurrencyExceptionHandler>();

        return services;
    }
}
