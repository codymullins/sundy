using Microsoft.EntityFrameworkCore;

namespace Sundy.Core;

public class BlockingEngine
{
    private readonly SundyDbContext _db;
    private readonly ICalendarProvider _provider;
    
    public async Task<CalendarEvent> CreateEventWithBlockingAsync(
        string sourceCalendarId, 
        CalendarEvent sourceEvent)
    {
        // 1. Create the source event
        var created = await _provider.CreateEventAsync(sourceCalendarId, sourceEvent);
        
        // 2. Get source calendar's blocking settings
        var sourceCal = await _db.Calendars.FindAsync(sourceCalendarId);
        if (sourceCal?.EnableBlocking != true)
            return created; // Blocking disabled
        
        // 3. Find all calendars that should receive blocks
        var targetCalendars = await _db.Calendars
            .Where(c => c.Id != sourceCalendarId 
                        && c.ReceiveBlocks)
            .ToListAsync();
        
        if (!targetCalendars.Any())
            return created;
        
        // 4. Create blocking events
        var blockingRelationship = new BlockingRelationship
        {
            Id = Guid.NewGuid().ToString(),
            SourceCalendarId = sourceCalendarId,
            SourceEventId = created.Id,
            BlockedEvents = new()
        };
        
        foreach (var targetCal in targetCalendars)
        {
            var blockEvent = new CalendarEvent
            {
                Title = $"🔒 Busy ({created.Title})",
                StartTime = created.StartTime,
                EndTime = created.EndTime,
                Description = $"Time blocked by Sundy\nSource: {sourceCal.Name}",
                IsBlockingEvent = true,
                SourceEventId = created.Id
            };
            
            var blocked = await _provider.CreateEventAsync(targetCal.Id, blockEvent);
            
            blockingRelationship.BlockedEvents.Add(new BlockedEvent
            {
                TargetCalendarId = targetCal.Id,
                TargetEventId = blocked.Id
            });
        }
        
        // 5. Store relationship
        _db.BlockingRelationships.Add(blockingRelationship);
        await _db.SaveChangesAsync();
        
        return created;
    }
    
    public async Task UpdateEventWithBlockingAsync(
        string calendarId, 
        CalendarEvent updatedEvent)
    {
        // 1. Update the source event
        await _provider.UpdateEventAsync(calendarId, updatedEvent);
        
        // 2. Find blocking relationship
        var relationship = await _db.BlockingRelationships
            .Include(r => r.BlockedEvents)
            .FirstOrDefaultAsync(r => r.SourceEventId == updatedEvent.Id);
        
        if (relationship == null)
            return; // No blocking events to update
        
        // 3. Update all blocking events
        foreach (var blocked in relationship.BlockedEvents)
        {
            var blockEvent = await _db.Events.FindAsync(blocked.TargetEventId);
            if (blockEvent != null)
            {
                blockEvent.Title = $"🔒 Busy ({updatedEvent.Title})";
                blockEvent.StartTime = updatedEvent.StartTime;
                blockEvent.EndTime = updatedEvent.EndTime;
            }
        }
        
        await _db.SaveChangesAsync();
    }
    
    public async Task DeleteEventWithBlockingAsync(string calendarId, string eventId)
    {
        // 1. Find blocking relationship
        var relationship = await _db.BlockingRelationships
            .Include(r => r.BlockedEvents)
            .FirstOrDefaultAsync(r => r.SourceEventId == eventId);
        
        // 2. Delete all blocking events
        if (relationship != null)
        {
            foreach (var blocked in relationship.BlockedEvents)
            {
                await _provider.DeleteEventAsync(blocked.TargetCalendarId, blocked.TargetEventId);
            }
            
            _db.BlockingRelationships.Remove(relationship);
        }
        
        // 3. Delete the source event
        await _provider.DeleteEventAsync(calendarId, eventId);
        
        await _db.SaveChangesAsync();
    }
}