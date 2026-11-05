using Microsoft.EntityFrameworkCore;
using ReserveFlow.Inventory.Application.Abstractions;
using ReserveFlow.Inventory.Application.Exceptions;
using ReserveFlow.Inventory.Domain.Inventories;

namespace ReserveFlow.Inventory.Infrastructure.Persistence
{
    public sealed class InventoryDbContext : DbContext, IInventoryUnitOfWork
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        public DbSet<RoomInventory> RoomInventories => Set<RoomInventory>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InventoryConcurrencyException("Inventory was modified by another transaction.", ex);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("inventory");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
