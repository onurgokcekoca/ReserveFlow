namespace ReserveFlow.Inventory.Application.Abstractions
{
    public interface IInventoryUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
