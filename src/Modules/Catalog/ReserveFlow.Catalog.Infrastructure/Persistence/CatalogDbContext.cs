using Microsoft.EntityFrameworkCore;
using ReserveFlow.Catalog.Application.Abstractions;
using ReserveFlow.Catalog.Domain.Properties;
using ReserveFlow.Catalog.Infrastructure.Outbox;

namespace ReserveFlow.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext : DbContext, ICatalogUnitOfWork
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
