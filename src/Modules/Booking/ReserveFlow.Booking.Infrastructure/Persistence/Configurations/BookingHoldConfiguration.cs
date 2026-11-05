using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Booking.Domain.Bookings;

namespace ReserveFlow.Booking.Infrastructure.Persistence.Configurations
{
    internal sealed class BookingHoldConfiguration : IEntityTypeConfiguration<BookingHold>
    {
        public void Configure(EntityTypeBuilder<BookingHold> builder)
        {
            builder.ToTable("booking_holds");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .ValueGeneratedNever();

            builder.Property<Guid>("PropertyId")
                .HasColumnName("property_id")
                .IsRequired();

            builder.Property<Guid>("RoomTypeId")
                .HasColumnName("room_type_id")
                .IsRequired();

            builder.Property<DateOnly>("CheckIn")
                .HasColumnName("check_in")
                .IsRequired();

            builder.Property<DateOnly>("CheckOut")
                .HasColumnName("check_out")
                .IsRequired();

            builder.Property<int>("Rooms")
                .HasColumnName("rooms")
                .IsRequired();

            builder.Property<int>("Guests")
                .HasColumnName("guests")
                .IsRequired();

            builder.Property<StatusType>("Status")
                .HasColumnName("status")
                .IsRequired();

            builder.Property<DateTime>("CreatedAtUtc")
                .HasColumnName("created_at_utc")
                .IsRequired();

            builder.Property<DateTime>("ExpiresAtUtc")
                .HasColumnName("expires_at_utc")
                .IsRequired();

            builder.Property("Version")
                .HasColumnName("version")
                .HasDefaultValue(1)
                .IsConcurrencyToken()
                .IsRequired();
        }
    }
}
