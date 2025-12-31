using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Sundy.Core.Calendars.Sync;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Calendar provider for Microsoft Outlook via Microsoft Graph API.
/// Supports multiple accounts via IMicrosoftAccountManager.
/// </summary>
public class OutlookCalendarProvider(
    IMicrosoftAccountManager accountManager,
    ILogger<OutlookCalendarProvider> logger)
{
    /// <summary>
    /// Gets all calendars from a specific Outlook account.
    /// </summary>
    public async Task<List<OutlookCalendarInfo>> GetCalendarsAsync(string accountId, CancellationToken ct = default)
    {
        var client = await accountManager.GetClientForAccountAsync(accountId, ct);
        if (client == null)
        {
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
            if (ex is Microsoft.Graph.Models.ODataErrors.ODataError odataError)
            {
                logger.LogError(ex, "Failed to get calendars - OData error: {Code} - {Message}",
                    odataError.Error?.Code, odataError.Error?.Message);
            }
            else
            {
                logger.LogError(ex, "Failed to get calendars for account {AccountId}", accountId);
            }
            return [];
        }
    }

    /// <summary>
    /// Gets events from a specific Outlook calendar.
    /// </summary>
    /// <param name="accountId">The connected account ID.</param>
    /// <param name="graphCalendarId">The Graph API calendar ID (not the local ID).</param>
    /// <param name="start">Start of date range.</param>
    /// <param name="end">End of date range.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<List<CalendarEvent>> GetEventsAsync(string accountId, string graphCalendarId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default)
    {
        var client = await accountManager.GetClientForAccountAsync(accountId, ct);
        if (client == null)
        {
            return [];
        }

        try
        {
            // Use calendarView to get events in a date range (handles recurring events)
            var events = await client.Me.Calendars[graphCalendarId].CalendarView
                .GetAsync(requestConfig =>
                {
                    requestConfig.QueryParameters.StartDateTime = start.ToString("o");
                    requestConfig.QueryParameters.EndDateTime = end.ToString("o");
                    requestConfig.QueryParameters.Select = ["id", "subject", "start", "end", "location", "bodyPreview", "isAllDay"];
                    requestConfig.QueryParameters.Orderby = ["start/dateTime"];
                    requestConfig.QueryParameters.Top = 500;
                }, cancellationToken: ct);

            return events?.Value?.Select(e => MapToCalendarEvent(e, graphCalendarId)).ToList() ?? [];
        }
        catch (Exception ex)
        {
            if (ex is Microsoft.Graph.Models.ODataErrors.ODataError odataError)
            {
                logger.LogError(ex, "Failed to get events - OData error: {Code} - {Message}",
                    odataError.Error?.Code, odataError.Error?.Message);
            }
            else
            {
                logger.LogError(ex, "Failed to get events for calendar {CalendarId}", graphCalendarId);
            }
            return [];
        }
    }

    /// <summary>
    /// Creates an event in a specific Outlook calendar.
    /// </summary>
    public async Task<CalendarEvent> CreateEventAsync(string accountId, string graphCalendarId, CalendarEvent evt, CancellationToken ct = default)
    {
        var client = await accountManager.GetClientForAccountAsync(accountId, ct);
        if (client == null)
        {
            throw new InvalidOperationException("Not authenticated with Microsoft Graph");
        }

        var graphEvent = MapToGraphEvent(evt);
        var created = await client.Me.Calendars[graphCalendarId].Events.PostAsync(graphEvent, cancellationToken: ct);

        if (created == null)
        {
            throw new InvalidOperationException("Failed to create event in Outlook calendar");
        }

        return MapToCalendarEvent(created, graphCalendarId);
    }

    /// <summary>
    /// Updates an event in a specific Outlook calendar.
    /// </summary>
    public async Task<CalendarEvent> UpdateEventAsync(string accountId, string graphCalendarId, CalendarEvent evt, CancellationToken ct = default)
    {
        var client = await accountManager.GetClientForAccountAsync(accountId, ct);
        if (client == null)
        {
            throw new InvalidOperationException("Not authenticated with Microsoft Graph");
        }

        if (string.IsNullOrEmpty(evt.Id))
        {
            throw new ArgumentException("Event ID is required for update", nameof(evt));
        }

        var graphEvent = MapToGraphEvent(evt);
        var updated = await client.Me.Calendars[graphCalendarId].Events[evt.Id].PatchAsync(graphEvent, cancellationToken: ct);

        if (updated == null)
        {
            throw new InvalidOperationException("Failed to update event in Outlook calendar");
        }

        return MapToCalendarEvent(updated, graphCalendarId);
    }

    /// <summary>
    /// Deletes an event from a specific Outlook calendar.
    /// </summary>
    public async Task DeleteEventAsync(string accountId, string graphCalendarId, string eventId, CancellationToken ct = default)
    {
        var client = await accountManager.GetClientForAccountAsync(accountId, ct);
        if (client == null)
        {
            throw new InvalidOperationException("Not authenticated with Microsoft Graph");
        }

        await client.Me.Calendars[graphCalendarId].Events[eventId].DeleteAsync(cancellationToken: ct);
    }

    /// <summary>
    /// Gets events using delta sync for efficient change tracking.
    /// If deltaToken is null, performs a full sync for the given date range.
    /// </summary>
    /// <param name="accountId">The connected account ID.</param>
    /// <param name="graphCalendarId">The Graph API calendar ID.</param>
    /// <param name="deltaToken">The delta token from the previous sync (null for initial sync).</param>
    /// <param name="start">Start of date range (only used for initial sync).</param>
    /// <param name="end">End of date range (only used for initial sync).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Delta sync result with added/modified events, deleted event IDs, and next delta token.</returns>
    public async Task<DeltaSyncResult> GetEventsDeltaAsync(
        string accountId,
        string graphCalendarId,
        string? deltaToken,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken ct = default)
    {
        var client = await accountManager.GetClientForAccountAsync(accountId, ct);
        if (client == null)
        {
            return new DeltaSyncResult { RequiresFullSync = true };
        }

        try
        {
            // If no delta token, do a full sync using calendarView
            if (string.IsNullOrEmpty(deltaToken))
            {
                return await PerformFullSyncAsync(client, graphCalendarId, start, end, ct);
            }

            // Use the delta endpoint with the stored token
            return await PerformDeltaSyncAsync(client, graphCalendarId, deltaToken, ct);
        }
        catch (Microsoft.Graph.Models.ODataErrors.ODataError odataError)
        {
            // Delta token may be expired or invalid
            if (odataError.Error?.Code == "InvalidSyncStateData" ||
                odataError.Error?.Code == "ResyncRequired" ||
                odataError.Error?.Code == "SyncStateNotFound")
            {
                logger.LogWarning("Delta token expired or invalid for calendar {CalendarId}, requiring full sync", graphCalendarId);
                return new DeltaSyncResult { RequiresFullSync = true };
            }
            logger.LogError(odataError, "Failed to get events delta - OData error: {Code} - {Message}",
                odataError.Error?.Code, odataError.Error?.Message);
            throw;
        }
    }

    private async Task<DeltaSyncResult> PerformFullSyncAsync(
        Microsoft.Graph.GraphServiceClient client,
        string graphCalendarId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken ct)
    {
        var events = new List<CalendarEvent>();
        string? nextLink = null;

        // Initial request
        var response = await client.Me.Calendars[graphCalendarId].CalendarView
            .GetAsync(requestConfig =>
            {
                requestConfig.QueryParameters.StartDateTime = start.ToString("o");
                requestConfig.QueryParameters.EndDateTime = end.ToString("o");
                requestConfig.QueryParameters.Select = ["id", "subject", "start", "end", "location", "bodyPreview", "isAllDay", "lastModifiedDateTime"];
                requestConfig.QueryParameters.Orderby = ["start/dateTime"];
                requestConfig.QueryParameters.Top = 100;
            }, cancellationToken: ct);

        if (response?.Value != null)
        {
            events.AddRange(response.Value.Select(e => MapToCalendarEventWithModified(e, graphCalendarId)));
        }

        nextLink = response?.OdataNextLink;

        // Follow pagination
        while (!string.IsNullOrEmpty(nextLink) && !ct.IsCancellationRequested)
        {
            var nextResponse = await client.Me.Calendars[graphCalendarId].CalendarView
                .WithUrl(nextLink)
                .GetAsync(cancellationToken: ct);

            if (nextResponse?.Value != null)
            {
                events.AddRange(nextResponse.Value.Select(e => MapToCalendarEventWithModified(e, graphCalendarId)));
            }

            nextLink = nextResponse?.OdataNextLink;
        }

        // For initial sync, we generate a pseudo delta token from the current timestamp
        // The next sync will use the actual delta endpoint
        var pseudoDeltaToken = $"full_sync_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        return new DeltaSyncResult
        {
            AddedOrModified = events,
            DeletedEventIds = [], // Full sync doesn't identify deletions
            NextDeltaToken = pseudoDeltaToken,
            RequiresFullSync = false
        };
    }

    private async Task<DeltaSyncResult> PerformDeltaSyncAsync(
        Microsoft.Graph.GraphServiceClient client,
        string graphCalendarId,
        string deltaToken,
        CancellationToken ct)
    {
        var addedOrModified = new List<CalendarEvent>();
        var deletedIds = new List<string>();

        // Check if it's a pseudo token from full sync (start fresh with delta)
        if (deltaToken.StartsWith("full_sync_"))
        {
            // Get events from the last sync time forward using delta endpoint
            // For simplicity, we'll do a new full sync with delta tracking enabled
            var now = DateTimeOffset.UtcNow;
            var start = now.AddMonths(-3);
            var end = now.AddMonths(6);

            return await PerformFullSyncAsync(client, graphCalendarId, start, end, ct);
        }

        // Use actual delta endpoint (note: Calendar events delta requires specific setup)
        // The Graph API delta for calendar events is through the events collection
        try
        {
            var response = await client.Me.Calendars[graphCalendarId].Events
                .Delta
                .GetAsDeltaGetResponseAsync(requestConfig =>
                {
                    requestConfig.QueryParameters.Select = ["id", "subject", "start", "end", "location", "bodyPreview", "isAllDay", "lastModifiedDateTime"];
                }, cancellationToken: ct);

            string? nextLink = response?.OdataNextLink;
            string? newDeltaLink = response?.OdataDeltaLink;

            if (response?.Value != null)
            {
                foreach (var evt in response.Value)
                {
                    // Check if this is a deletion (indicated by @removed in response)
                    // Graph SDK handles this through the event having only an ID
                    if (evt.Subject == null && evt.Start == null && evt.End == null && evt.Id != null)
                    {
                        deletedIds.Add(evt.Id);
                    }
                    else
                    {
                        addedOrModified.Add(MapToCalendarEventWithModified(evt, graphCalendarId));
                    }
                }
            }

            // Follow pagination for delta
            while (!string.IsNullOrEmpty(nextLink) && !ct.IsCancellationRequested)
            {
                var nextResponse = await client.Me.Calendars[graphCalendarId].Events
                    .Delta
                    .WithUrl(nextLink)
                    .GetAsDeltaGetResponseAsync(cancellationToken: ct);

                if (nextResponse?.Value != null)
                {
                    foreach (var evt in nextResponse.Value)
                    {
                        if (evt.Subject == null && evt.Start == null && evt.End == null && evt.Id != null)
                        {
                            deletedIds.Add(evt.Id);
                        }
                        else
                        {
                            addedOrModified.Add(MapToCalendarEventWithModified(evt, graphCalendarId));
                        }
                    }
                }

                nextLink = nextResponse?.OdataNextLink;
                newDeltaLink = nextResponse?.OdataDeltaLink ?? newDeltaLink;
            }

            return new DeltaSyncResult
            {
                AddedOrModified = addedOrModified,
                DeletedEventIds = deletedIds,
                NextDeltaToken = newDeltaLink ?? deltaToken,
                RequiresFullSync = false
            };
        }
        catch (Exception ex)
        {
            // If delta fails, require full sync
            logger.LogWarning(ex, "Delta sync failed for calendar {CalendarId}, requiring full sync", graphCalendarId);
            return new DeltaSyncResult { RequiresFullSync = true };
        }
    }

    private static CalendarEvent MapToCalendarEventWithModified(Event graphEvent, string calendarId)
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
            SourceEventId = null,
            ExternalId = $"outlook_{graphEvent.Id}",
            ExternalModifiedAt = graphEvent.LastModifiedDateTime
        };
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
