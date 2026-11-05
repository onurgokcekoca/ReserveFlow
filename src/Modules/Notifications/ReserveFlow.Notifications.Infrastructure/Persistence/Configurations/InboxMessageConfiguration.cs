using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReserveFlow.Notifications.Infrastructure.Persistence.Entities;

namespace ReserveFlow.Notifications.Infrastructure.Persistence.Configurations
{
    internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.ToTable("inbox_messages");

            builder.HasKey(nm => nm.MessageId);
            builder.Property(nm => nm.MessageId)
                .HasColumnName("message_id")
                .ValueGeneratedNever();

            builder.Property(nm => nm.MessageType)
                .HasColumnName("message_type")
                .IsRequired();

            builder.Property(nm => nm.ProcessedAtUtc)
                .HasColumnName("processed_at_utc")
                .IsRequired();
        }
    }
}
