namespace ReserveFlow.Catalog.Application.Abstractions
{
    public interface ICatalogUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
