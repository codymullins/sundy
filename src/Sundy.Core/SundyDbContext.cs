using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sundy.Core;
public class SundyDbContextFactory : IDesignTimeDbContextFactory<SundyDbContext>
{
    public SundyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SundyDbContext>();
        optionsBuilder.UseSqlite("Data Source=sundy.db");

        return new SundyDbContext(optionsBuilder.Options);
    }
}
public class SundyDbContext(DbContextOptions<SundyDbContext> options) : DbContext(options)
{
    public DbSet<Calendar> Calendars => Set<Calendar>();
    public DbSet<CalendarEvent> Events => Set<CalendarEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Calendar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Color).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.EnableBlocking).IsRequired();
            entity.Property(e => e.ReceiveBlocks).IsRequired();
        });

        modelBuilder.Entity<CalendarEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CalendarId).IsRequired();
            entity.Property(e => e.IsBlockingEvent).IsRequired();

            // Store DateTimeOffset as ISO8601 string to preserve offset
            entity.Property(e => e.StartTime)
                .IsRequired()
                .HasConversion(
                    v => v.ToString("o"),
                    v => DateTimeOffset.Parse(v));

            entity.Property(e => e.EndTime)
                .IsRequired()
                .HasConversion(
                    v => v.ToString("o"),
                    v => DateTimeOffset.Parse(v));

            entity.HasOne<Calendar>()
                .WithMany()
                .HasForeignKey(e => e.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
