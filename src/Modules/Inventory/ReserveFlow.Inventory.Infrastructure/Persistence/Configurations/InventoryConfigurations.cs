using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Inventory.Domain.Inventories;

namespace ReserveFlow.Inventory.Infrastructure.Persistence.Configurations
{
    internal sealed class RoomInventoryConfiguration : IEntityTypeConfiguration<RoomInventory>
    {
        public void Configure(EntityTypeBuilder<RoomInventory> builder)
        {
            builder.ToTable("room_inventories", "inventory");

            builder.HasKey(ri => ri.Id);

            builder.Property(ri => ri.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(ri => ri.RoomTypeId)
                .HasColumnName("room_type_id")
                .IsRequired();

            builder.Property(ri => ri.Date)
                .HasColumnName("date")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(ri => ri.AvailableRooms)
                .HasColumnName("available_rooms")
                .IsRequired();

            builder.Property(ri => ri.TotalRooms)
                .HasColumnName("total_rooms")
                .IsRequired();

            builder.HasIndex(ri => new { ri.RoomTypeId, ri.Date })
                .IsUnique()
                .HasDatabaseName("ux_room_inventories_room_type_id_date");

            builder.Property(ri => ri.Version)
                .HasColumnName("version")
                .IsConcurrencyToken()
                .IsRequired();
        }
    }
}
