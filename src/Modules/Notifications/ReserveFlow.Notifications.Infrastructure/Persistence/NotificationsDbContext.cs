using Microsoft.EntityFrameworkCore;
using ReserveFlow.Notifications.Infrastructure.Persistence.Entities;

namespace ReserveFlow.Notifications.Infrastructure.Persistence
{
    public class NotificationsDbContext : DbContext
    {
        public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options)
        {
        }

        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
        public DbSet<BookingConfirmationNotification> BookingConfirmationNotifications => Set<BookingConfirmationNotification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("notifications");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
