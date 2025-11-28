using Microsoft.EntityFrameworkCore;

namespace Sundy.Core;

public class SundyDbContext(DbContextOptions<SundyDbContext> options) : 
    DbContext(options)
{
    public DbSet<Calendar> Calendars { get; set; }
    public DbSet<CalendarEvent> Events { get; set; }
    public DbSet<BlockingRelationship> BlockingRelationships { get; set; }
    public DbSet<BlockedEvent> BlockedEvents { get; set; }
    
}