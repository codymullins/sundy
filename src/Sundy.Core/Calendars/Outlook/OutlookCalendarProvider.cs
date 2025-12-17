using Microsoft.Graph.Models;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Calendar provider for Microsoft Outlook via Microsoft Graph API.
/// </summary>
public class OutlookCalendarProvider(MicrosoftGraphAuthService authService, OutlookGraphOptions? options = null)
    : ICalendarProvider
{
    private readonly OutlookGraphOptions _options = options ?? new OutlookGraphOptions();

    public bool IsConnected => authService.IsAuthenticated;
    public string? UserDisplayName => authService.UserDisplayName;

    /// <summary>
    /// Connects to Microsoft Graph by initiating authentication.
    /// </summary>
    public Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        // Choose authentication method based on configuration
        return _options.UseDeviceCodeFlow 
            ? authService.AuthenticateWithDeviceCodeAsync(ct) :
            authService.AuthenticateAsync(ct);
    }

    /// <summary>
    /// Disconnects from Microsoft Graph.
    /// </summary>
    public void Disconnect()
    {
        authService.SignOut();
    }

    /// <summary>
    /// Gets all calendars from the user's Outlook account.
    /// </summary>
    public async Task<List<OutlookCalendarInfo>> GetCalendarsAsync(CancellationToken ct = default)
    {
        var client = authService.GetClient();
        if (client == null)
        {
            // Log.Warning("Cannot get calendars - not authenticated");
            return [];
        }

        try
        {
            var calendars = await client.Me.Calendars.GetAsync(cancellationToken: ct);
            
            return calendars?.Value?.Select(c => new OutlookCalendarInfo
            {
                Id = c.Id ?? string.Empty,
                Name = c.Name ?? "Unnamed Calendar",
                Color = MapOutlookColor(c.Color),
                IsDefault = c.IsDefaultCalendar ?? false
            }).ToList() ?? [];
        }
        catch (Exception ex)
        {
            // Log more details about the error for better diagnostics
            if (ex is Microsoft.Graph.Models.ODataErrors.ODataError odataError)
            {
                // Log.Error(ex, "Failed to get calendars - OData error: {Code} - {Message}",
                //     odataError.Error?.Code, odataError.Error?.Message);
            }
            else
            {
                // Log.Error(ex, "Failed to get Outlook calendars");
            }
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<List<CalendarEvent>> GetEventsAsync(string calendarId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default)
    {
        var client = authService.GetClient();
        if (client == null)
        {
            // Log.Warning("Cannot get events - not authenticated");
            return [];
        }

        try
        {
            // Use calendarView to get events in a date range (handles recurring events)
            var events = await client.Me.Calendars[calendarId].CalendarView
                .GetAsync(requestConfig =>
                {
                    requestConfig.QueryParameters.StartDateTime = start.ToString("o");
                    requestConfig.QueryParameters.EndDateTime = end.ToString("o");
                    requestConfig.QueryParameters.Select = ["id", "subject", "start", "end", "location", "bodyPreview", "isAllDay"];
                    requestConfig.QueryParameters.Orderby = ["start/dateTime"];
                    requestConfig.QueryParameters.Top = 500;
                }, cancellationToken: ct);

            return events?.Value?.Select(e => MapToCalendarEvent(e, calendarId)).ToList() ?? [];
        }
        catch (Exception ex)
        {
            // Log more details about the error for better diagnostics
            if (ex is Microsoft.Graph.Models.ODataErrors.ODataError odataError)
            {
                // Log.Error(ex, "Failed to get events for calendar {CalendarId} - OData error: {Code} - {Message}",
                //     calendarId, odataError.Error?.Code, odataError.Error?.Message);
            }
            else
            {
                // Log.Error(ex, "Failed to get Outlook events for calendar {CalendarId}", calendarId);
            }
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<CalendarEvent> CreateEventAsync(string calendarId, CalendarEvent evt, CancellationToken ct = default)
    {
        var client = authService.GetClient();
        if (client == null)
        {
            throw new InvalidOperationException("Not authenticated with Microsoft Graph");
        }

        var graphEvent = MapToGraphEvent(evt);
        var created = await client.Me.Calendars[calendarId].Events.PostAsync(graphEvent, cancellationToken: ct);

        if (created == null)
        {
            throw new InvalidOperationException("Failed to create event in Outlook calendar");
        }
        
        return MapToCalendarEvent(created, calendarId);
    }

    /// <inheritdoc/>
    public async Task<CalendarEvent> UpdateEventAsync(string calendarId, CalendarEvent evt, CancellationToken ct = default)
    {
        var client = authService.GetClient();
        if (client == null)
        {
            throw new InvalidOperationException("Not authenticated with Microsoft Graph");
        }

        if (string.IsNullOrEmpty(evt.Id))
        {
            throw new ArgumentException("Event ID is required for update", nameof(evt));
        }

        var graphEvent = MapToGraphEvent(evt);
        var updated = await client.Me.Calendars[calendarId].Events[evt.Id].PatchAsync(graphEvent, cancellationToken: ct);

        if (updated == null)
        {
            throw new InvalidOperationException("Failed to update event in Outlook calendar");
        }
        
        return MapToCalendarEvent(updated, calendarId);
    }

    public async Task DeleteEventAsync(string calendarId, string eventId, CancellationToken ct = default)
    {
        var client = authService.GetClient();
        if (client == null)
        {
            throw new InvalidOperationException("Not authenticated with Microsoft Graph");
        }

        await client.Me.Calendars[calendarId].Events[eventId].DeleteAsync(cancellationToken: ct);
    }

    private static CalendarEvent MapToCalendarEvent(Event graphEvent, string calendarId)
    {
        var startTime = ParseGraphDateTime(graphEvent.Start);
        var endTime = ParseGraphDateTime(graphEvent.End);

        return new CalendarEvent
        {
            Id = graphEvent.Id,
            CalendarId = calendarId,
            Title = graphEvent.Subject ?? string.Empty,
            StartTime = startTime,
            EndTime = endTime,
            Description = graphEvent.BodyPreview,
            Location = graphEvent.Location?.DisplayName,
            IsBlockingEvent = false,
            SourceEventId = null
        };
    }

    private static Event MapToGraphEvent(CalendarEvent evt)
    {
        return new Event
        {
            Subject = evt.Title,
            Body = new ItemBody
            {
                ContentType = BodyType.Text,
                Content = evt.Description
            },
            Start = new DateTimeTimeZone
            {
                DateTime = evt.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                TimeZone = TimeZoneInfo.Local.Id
            },
            End = new DateTimeTimeZone
            {
                DateTime = evt.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                TimeZone = TimeZoneInfo.Local.Id
            },
            Location = new Location
            {
                DisplayName = evt.Location
            }
        };
    }

    private static DateTimeOffset ParseGraphDateTime(DateTimeTimeZone? dateTimeTimeZone)
    {
        if (dateTimeTimeZone == null || string.IsNullOrEmpty(dateTimeTimeZone.DateTime))
        {
            return DateTimeOffset.MinValue;
        }

        // Graph API returns datetime in the specified timezone
        if (DateTime.TryParse(dateTimeTimeZone.DateTime, out var dt))
        {
            // Try to get the timezone
            if (!string.IsNullOrEmpty(dateTimeTimeZone.TimeZone))
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(dateTimeTimeZone.TimeZone);
                    return new DateTimeOffset(dt, tz.GetUtcOffset(dt));
                }
                catch
                {
                    // Fall back to local time if timezone not found
                }
            }
            return new DateTimeOffset(dt, TimeZoneInfo.Local.GetUtcOffset(dt));
        }

        return DateTimeOffset.MinValue;
    }

    private static string MapOutlookColor(CalendarColor? color)
    {
        return color switch
        {
            CalendarColor.LightBlue => "#0078D4",
            CalendarColor.LightGreen => "#107C10",
            CalendarColor.LightOrange => "#FF8C00",
            CalendarColor.LightGray => "#737373",
            CalendarColor.LightYellow => "#FFB900",
            CalendarColor.LightTeal => "#008272",
            CalendarColor.LightPink => "#E3008C",
            CalendarColor.LightBrown => "#8E562E",
            CalendarColor.LightRed => "#D13438",
            CalendarColor.Auto => "#0078D4",
            _ => "#0078D4"
        };
    }
}
