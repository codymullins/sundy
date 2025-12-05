using Sundy.Core;

namespace Sundy.Test.TestHelpers;

public static class TestDataBuilder
{
    public static Calendar CreateTestCalendar(
        string? id = null,
        string? name = null,
        string color = "#FF0000",
        CalendarType type = CalendarType.Local,
        bool enableBlocking = false,
        bool receiveBlocks = false)
    {
        return new Calendar
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name ?? "Test Calendar",
            Color = color,
            Type = type,
            EnableBlocking = enableBlocking,
            ReceiveBlocks = receiveBlocks
        };
    }

    public static CalendarEvent CreateTestEvent(
        string? calendarId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        string title = "Test Event",
        string? location = null,
        string? description = null,
        bool isBlockingEvent = false,
        string? sourceEventId = null)
    {
        var actualCalendarId = calendarId ?? Guid.NewGuid().ToString();
        var actualStartTime = startTime ?? DateTime.Now;
        var actualEndTime = endTime ?? actualStartTime.AddHours(1);

        return new CalendarEvent
        {
            Id = Guid.NewGuid().ToString(),
            CalendarId = actualCalendarId,
            Title = title,
            StartTime = new DateTimeOffset(actualStartTime),
            EndTime = new DateTimeOffset(actualEndTime),
            Location = location,
            Description = description,
            IsBlockingEvent = isBlockingEvent,
            SourceEventId = sourceEventId
        };
    }

    public static CalendarEvent CreateTestEventAllDay(
        string? calendarId = null,
        DateTime? date = null,
        string title = "All Day Event")
    {
        var actualDate = date ?? DateTime.Today;
        var midnight = new DateTime(actualDate.Year, actualDate.Month, actualDate.Day, 0, 0, 0, DateTimeKind.Local);
        var nextMidnight = midnight.AddDays(1);

        return CreateTestEvent(
            calendarId: calendarId,
            startTime: midnight,
            endTime: nextMidnight,
            title: title);
    }

    public static CalendarEvent CreateTestEventSpanningMidnight(
        string? calendarId = null,
        DateTime? startDate = null,
        TimeSpan? startTime = null,
        TimeSpan? endTime = null,
        string title = "Midnight Spanning Event")
    {
        var actualDate = startDate ?? DateTime.Today;
        var actualStartTime = startTime ?? TimeSpan.FromHours(23);
        var actualEndTime = endTime ?? TimeSpan.FromHours(1);

        var start = actualDate.Date.Add(actualStartTime);
        var end = actualDate.Date.AddDays(1).Add(actualEndTime);

        return CreateTestEvent(
            calendarId: calendarId,
            startTime: start,
            endTime: end,
            title: title);
    }

    public static List<CalendarEvent> CreateTestEventsInRange(
        string calendarId,
        DateTime rangeStart,
        int count,
        TimeSpan? eventDuration = null)
    {
        var events = new List<CalendarEvent>();
        var duration = eventDuration ?? TimeSpan.FromHours(1);
        var currentStart = rangeStart;

        for (int i = 0; i < count; i++)
        {
            events.Add(CreateTestEvent(
                calendarId: calendarId,
                startTime: currentStart,
                endTime: currentStart.Add(duration),
                title: $"Event {i + 1}"));

            currentStart = currentStart.AddHours(2); // Space them out
        }

        return events;
    }

    public static CalendarEvent CreateTestEventAtMidnight(
        string? calendarId = null,
        DateTime? date = null,
        TimeSpan? duration = null,
        string title = "Midnight Event")
    {
        var actualDate = date ?? DateTime.Today;
        var midnight = new DateTime(actualDate.Year, actualDate.Month, actualDate.Day, 0, 0, 0, DateTimeKind.Local);
        var actualDuration = duration ?? TimeSpan.FromHours(1);

        return CreateTestEvent(
            calendarId: calendarId,
            startTime: midnight,
            endTime: midnight.Add(actualDuration),
            title: title);
    }

    public static CalendarEvent CreateTestEventAtEndOfDay(
        string? calendarId = null,
        DateTime? date = null,
        string title = "End of Day Event")
    {
        var actualDate = date ?? DateTime.Today;
        var endOfDay = new DateTime(actualDate.Year, actualDate.Month, actualDate.Day, 23, 59, 0, DateTimeKind.Local);

        return CreateTestEvent(
            calendarId: calendarId,
            startTime: endOfDay.AddMinutes(-30),
            endTime: endOfDay,
            title: title);
    }
}
