using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Notifications.Infrastructure.Persistence.Entities;

namespace ReserveFlow.Notifications.Infrastructure.Persistence.Configurations
{
    internal sealed class BookingConfirmationNotificationConfiguration : IEntityTypeConfiguration<BookingConfirmationNotification>
    {
        public void Configure(EntityTypeBuilder<BookingConfirmationNotification> builder)
        {
            builder.ToTable("booking_confirmation_notifications");

            builder.HasKey(bcn => bcn.Id);
            builder.Property(bcn => bcn.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.HasIndex(bcn => bcn.MessageId)
                .IsUnique();

            builder.Property(bcn => bcn.MessageId)
                .HasColumnName("message_id")
                .IsRequired();

            builder.Property(bcn => bcn.BookingId)
                .HasColumnName("booking_id")
                .IsRequired();

            builder.Property(bcn => bcn.PropertyId)
                .HasColumnName("property_id")
                .IsRequired();

            builder.Property(bcn => bcn.OccurredAtUtc)
                .HasColumnName("occurred_at_utc")
                .IsRequired();

            builder.Property(bcn => bcn.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();
        }
    }
}
