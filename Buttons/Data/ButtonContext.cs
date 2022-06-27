using Microsoft.EntityFrameworkCore;

namespace Buttons.Data
{
    public class ButtonContext : DbContext
    {
        public DbSet<Button> Buttons => Set<Button>();

        public ButtonContext(DbContextOptions<ButtonContext> dbContextOptions) : base(dbContextOptions)
        {
            ChangeTracker.Tracked += ChangeTrackerTracked;
            ChangeTracker.StateChanged += ChangeTrackerStateChanged;
        }

        private void ChangeTrackerTracked(object? sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityTrackedEventArgs e)
        {
            if (!e.FromQuery && e.Entry.State == EntityState.Added && e.Entry.Entity is DateEntity dateEntity)
            {
                dateEntity.Created = DateTime.Now;
                dateEntity.LastModified = DateTime.Now;
            }
        }

        private void ChangeTrackerStateChanged(object? sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityStateChangedEventArgs e)
        {
            if (e.NewState == EntityState.Modified && e.Entry.Entity is DateEntity dateEntity)
            {
                dateEntity.LastModified = DateTime.Now;
            }
        }
    }
}
