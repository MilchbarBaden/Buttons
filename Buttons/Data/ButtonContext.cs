using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Buttons.Data
{
    public class ButtonContext : DbContext
    {
        public DbSet<Button> Buttons => Set<Button>();
        public DbSet<Owner> Owners => Set<Owner>();

        public ButtonContext(DbContextOptions<ButtonContext> dbContextOptions) : base(dbContextOptions)
        {
            SavingChanges += ContextSavingChanges;
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        private void ContextSavingChanges(object? sender, SavingChangesEventArgs e)
        {
            ChangeTracker.DetectChanges();

            foreach (var entry in ChangeTracker.Entries<DateEntity>().Where(IsModified))
            {
                entry.Entity.LastModified = DateTime.Now;
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Created = DateTime.Now;
                }
                else if (entry.State == EntityState.Unchanged)
                {
                    // Set state to modified, if change originates from an owned entity.
                    // This is required for SaveChanges to update the changed LastModified propoerty
                    // in the database without running ChangeTracker.DetectChanges() again.
                    entry.State = EntityState.Modified;
                }
            }
        }

        private static bool IsModified(EntityEntry entry) =>
            entry.State == EntityState.Modified || entry.State == EntityState.Added ||
            entry.References.Any(r => r.TargetEntry != null && r.TargetEntry.Metadata.IsOwned() && IsModified(r.TargetEntry));
    }
}
