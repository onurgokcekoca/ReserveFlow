using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Catalog.Domain.Properties;

namespace ReserveFlow.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("properties");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(p => p.ProviderId)
            .HasColumnName("provider_id")
            .IsRequired();

        builder.Property(p => p.PropertyType)
            .HasColumnName("property_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.OwnsOne(p => p.Location, locationBuilder =>
        {
            locationBuilder.Property(l => l.Country)
                .HasColumnName("country")
                .HasMaxLength(100)
                .IsRequired();

            locationBuilder.Property(l => l.City)
                .HasColumnName("city")
                .HasMaxLength(100)
                .IsRequired();

            locationBuilder.Property(l => l.AddressLine)
                .HasColumnName("address_line")
                .HasMaxLength(500)
                .IsRequired();

            locationBuilder.Property(l => l.Latitude)
                .HasColumnName("latitude");

            locationBuilder.Property(l => l.Longitude)
                .HasColumnName("longitude");
        });

        builder.Navigation(p => p.Location)
            .IsRequired();

        builder.HasMany(p => p.RoomTypes)
            .WithOne()
            .HasForeignKey("PropertyId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.RoomTypes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
