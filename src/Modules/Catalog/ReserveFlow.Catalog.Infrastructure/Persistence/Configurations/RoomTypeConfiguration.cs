using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Catalog.Domain.Properties;

namespace ReserveFlow.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("room_types");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .ValueGeneratedNever();

        builder.Property(rt => rt.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(rt => rt.Capacity)
            .HasColumnName("capacity")
            .IsRequired();

        builder.Property(rt => rt.TotalRooms)
            .HasColumnName("total_rooms")
            .IsRequired();

        builder.Property<Guid>("PropertyId")
            .HasColumnName("property_id")
            .IsRequired();

        builder.OwnsOne(rt => rt.BasePrice, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("base_price_amount")
                .IsRequired();

            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("base_price_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(rt => rt.BasePrice)
            .IsRequired();
    }
}
