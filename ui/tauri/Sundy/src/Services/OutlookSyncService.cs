using Microsoft.Extensions.Logging;
using Sundy.Core;
using Sundy.Core.Calendars.Outlook;
using Sundy.Core.Calendars.Sync;
using Sundy.Core.Settings;

namespace Sundy.Services;

/// <summary>
/// Background service for syncing Outlook calendars using a Timer.
/// WASM-compatible implementation (no IHostedService).
/// </summary>
public class OutlookSyncService : IOutlookSyncService, IAsyncDisposable
{
    private readonly OutlookCalendarProvider _outlookProvider;
    private readonly ICalendarStore _calendarStore;
    private readonly IEventStore _eventStore;
    private readonly ISyncStateManager _stateManager;
    private readonly ISyncDeltaStore _deltaStore;
    private readonly IConnectedAccountStore _accountStore;
    private readonly ISettingsService _settings;
    private readonly ILogger<OutlookSyncService> _logger;

    private Timer? _syncTimer;
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private CancellationTokenSource? _cts;

    private int _syncIntervalMinutes = 5;

    public event Action<int>? OnIntervalChanged;

    public OutlookSyncService(
        OutlookCalendarProvider outlookProvider,
        ICalendarStore calendarStore,
        IEventStore eventStore,
        ISyncStateManager stateManager,
        ISyncDeltaStore deltaStore,
        IConnectedAccountStore accountStore,
        ISettingsService settings,
        ILogger<OutlookSyncService> logger)
    {
        _outlookProvider = outlookProvider;
        _calendarStore = calendarStore;
        _eventStore = eventStore;
        _stateManager = stateManager;
        _deltaStore = deltaStore;
        _accountStore = accountStore;
        _settings = settings;
        _logger = logger;
    }

    public bool IsRunning => _syncTimer != null;

    public int SyncIntervalMinutes
    {
        get => _syncIntervalMinutes;
        set
        {
            if (_syncIntervalMinutes != value)
            {
                _syncIntervalMinutes = value;
                OnIntervalChanged?.Invoke(value);

                // Restart timer with new interval if running
                if (_syncTimer != null)
                {
                    _syncTimer.Change(
                        TimeSpan.FromMinutes(value),
                        TimeSpan.FromMinutes(value));
                }
            }
        }
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_syncTimer != null) return;

        // Load interval from settings
        var interval = await _settings.GetSyncIntervalMinutesAsync(ct);
        _syncIntervalMinutes = interval > 0 ? interval : 5;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Start timer with initial delay of 10 seconds, then run at configured interval
        _syncTimer = new Timer(
            async _ => await SyncAllCalendarsAsync(),
            null,
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMinutes(_syncIntervalMinutes));

        _stateManager.AddLogEntry(new SyncLogEntry
        {
            Level = SyncLogLevel.Info,
            Message = $"Outlook sync started (interval: {_syncIntervalMinutes} min)"
        });

        _logger.LogInformation("Outlook sync service started with interval: {Interval} minutes", _syncIntervalMinutes);
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (_syncTimer != null)
        {
            await _syncTimer.DisposeAsync();
            _syncTimer = null;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _stateManager.AddLogEntry(new SyncLogEntry
        {
            Level = SyncLogLevel.Info,
            Message = "Outlook sync stopped"
        });

        _logger.LogInformation("Outlook sync service stopped");
    }

    public async Task SyncNowAsync(string? calendarId = null, CancellationToken ct = default)
    {
        if (calendarId != null)
        {
            var calendars = await _calendarStore.GetAllAsync(ct);
            var calendar = calendars.FirstOrDefault(c => c.Id == calendarId);
            if (calendar != null && !string.IsNullOrEmpty(calendar.ExternalAccountId))
            {
                var account = await _accountStore.GetByIdAsync(calendar.ExternalAccountId, ct);
                if (account != null)
                {
                    await SyncCalendarAsync(calendar, account, ct);
                }
            }
        }
        else
        {
            await SyncAllCalendarsAsync();
        }
    }

    private async Task SyncAllCalendarsAsync()
    {
        // Use tryWait to skip if sync is already in progress
        if (!await _syncLock.WaitAsync(0))
        {
            _logger.LogDebug("Sync already in progress, skipping");
            return;
        }

        try
        {
            var ct = _cts?.Token ?? default;

            // Get all connected Microsoft accounts
            var accounts = await _accountStore.GetByProviderTypeAsync(ProviderType.Microsoft, ct);
            var connectedAccounts = accounts.Where(a => a.Status == AccountStatus.Connected).ToList();

            if (connectedAccounts.Count == 0)
            {
                _logger.LogDebug("No connected Microsoft accounts, skipping sync");
                return;
            }

            // Get all calendars
            var allCalendars = await _calendarStore.GetAllAsync(ct);

            foreach (var account in connectedAccounts)
            {
                // Get calendars for this account
                var accountCalendars = allCalendars
                    .Where(c => c.ExternalAccountId == account.Id && c.Type == CalendarType.Microsoft)
                    .ToList();

                foreach (var calendar in accountCalendars)
                {
                    if (ct.IsCancellationRequested) break;

                    try
                    {
                        await SyncCalendarAsync(calendar, account, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync calendar {CalendarId}", calendar.Id);
                        _stateManager.UpdateCalendarState(calendar.Id, new CalendarSyncState
                        {
                            CalendarId = calendar.Id,
                            Status = SyncStatus.Error,
                            LastSyncTime = DateTime.UtcNow,
                            ErrorMessage = ex.Message
                        });
                        _stateManager.AddLogEntry(new SyncLogEntry
                        {
                            CalendarId = calendar.Id,
                            CalendarName = calendar.Name,
                            Level = SyncLogLevel.Error,
                            Message = $"Sync failed: {ex.Message}"
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during calendar sync");
            _stateManager.AddLogEntry(new SyncLogEntry
            {
                Level = SyncLogLevel.Error,
                Message = $"Sync cycle failed: {ex.Message}"
            });
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async Task SyncCalendarAsync(Calendar calendar, ConnectedAccount account, CancellationToken ct)
    {
        // Update state to syncing
        _stateManager.UpdateCalendarState(calendar.Id, new CalendarSyncState
        {
            CalendarId = calendar.Id,
            Status = SyncStatus.Syncing
        });
        _stateManager.AddLogEntry(new SyncLogEntry
        {
            CalendarId = calendar.Id,
            CalendarName = calendar.Name,
            Level = SyncLogLevel.Info,
            Message = "Starting sync..."
        });

        try
        {
            // Extract Graph calendar ID from our internal ID format
            // Our calendars use: "outlook_{graphCalendarId}"
            var graphCalendarId = calendar.Id.StartsWith("outlook_")
                ? calendar.Id.Substring(8)
                : calendar.Id;

            // Get delta token if available
            var deltaToken = await _deltaStore.GetDeltaTokenAsync(calendar.Id, ct);

            // Define sync window
            var now = DateTimeOffset.UtcNow;
            var start = now.AddMonths(-1);
            var end = now.AddMonths(6);

            // Fetch changes from Outlook
            var deltaResult = await _outlookProvider.GetEventsDeltaAsync(
                account.Id,
                graphCalendarId,
                deltaToken,
                start,
                end,
                ct);

            // Check if full sync is required
            if (deltaResult.RequiresFullSync && !string.IsNullOrEmpty(deltaToken))
            {
                // Clear the token and retry
                await _deltaStore.ClearDeltaTokenAsync(calendar.Id, ct);
                deltaResult = await _outlookProvider.GetEventsDeltaAsync(
                    account.Id,
                    graphCalendarId,
                    null,
                    start,
                    end,
                    ct);
            }

            // Reconcile events
            var (added, updated, deleted) = await ReconcileEventsAsync(calendar, deltaResult, ct);

            // Save new delta token
            if (!string.IsNullOrEmpty(deltaResult.NextDeltaToken))
            {
                await _deltaStore.SaveDeltaTokenAsync(calendar.Id, deltaResult.NextDeltaToken, ct);
            }

            // Update state to success
            var syncState = new CalendarSyncState
            {
                CalendarId = calendar.Id,
                Status = SyncStatus.Success,
                LastSyncTime = DateTime.UtcNow,
                EventsAdded = added,
                EventsUpdated = updated,
                EventsDeleted = deleted
            };
            _stateManager.UpdateCalendarState(calendar.Id, syncState);

            // Log success
            var message = $"Sync complete: +{added} ~{updated} -{deleted}";
            _stateManager.AddLogEntry(new SyncLogEntry
            {
                CalendarId = calendar.Id,
                CalendarName = calendar.Name,
                Level = SyncLogLevel.Success,
                Message = message
            });

            _logger.LogInformation("Synced calendar {CalendarName}: {Added} added, {Updated} updated, {Deleted} deleted",
                calendar.Name, added, updated, deleted);
        }
        catch (Exception ex)
        {
            // Update state to error
            _stateManager.UpdateCalendarState(calendar.Id, new CalendarSyncState
            {
                CalendarId = calendar.Id,
                Status = SyncStatus.Error,
                LastSyncTime = DateTime.UtcNow,
                ErrorMessage = ex.Message
            });

            throw;
        }
    }

    private async Task<(int added, int updated, int deleted)> ReconcileEventsAsync(
        Calendar calendar,
        DeltaSyncResult deltaResult,
        CancellationToken ct)
    {
        int added = 0, updated = 0, deleted = 0;

        // Get existing events for this calendar to track deletions
        var existingEvents = await _eventStore.GetByCalendarIdAsync(calendar.Id, ct);
        var existingByExternalId = existingEvents
            .Where(e => !string.IsNullOrEmpty(e.ExternalId))
            .ToDictionary(e => e.ExternalId!, e => e);
        var processedExternalIds = new HashSet<string>();

        // Process added/modified events
        foreach (var graphEvent in deltaResult.AddedOrModified)
        {
            if (string.IsNullOrEmpty(graphEvent.ExternalId)) continue;

            processedExternalIds.Add(graphEvent.ExternalId);

            if (existingByExternalId.TryGetValue(graphEvent.ExternalId, out var existing))
            {
                // Check if modified (compare timestamps or just update)
                if (graphEvent.ExternalModifiedAt > existing.ExternalModifiedAt ||
                    existing.ExternalModifiedAt == null)
                {
                    // Update existing event
                    existing.Title = graphEvent.Title;
                    existing.StartTime = graphEvent.StartTime;
                    existing.EndTime = graphEvent.EndTime;
                    existing.Description = graphEvent.Description;
                    existing.Location = graphEvent.Location;
                    existing.ExternalModifiedAt = graphEvent.ExternalModifiedAt;

                    await _eventStore.UpdateEventAsync(existing, ct);
                    updated++;
                }
            }
            else
            {
                // Create new event
                var newEvent = new CalendarEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    CalendarId = calendar.Id,
                    Title = graphEvent.Title,
                    StartTime = graphEvent.StartTime,
                    EndTime = graphEvent.EndTime,
                    Description = graphEvent.Description,
                    Location = graphEvent.Location,
                    IsBlockingEvent = false,
                    ExternalId = graphEvent.ExternalId,
                    ExternalModifiedAt = graphEvent.ExternalModifiedAt
                };

                await _eventStore.CreateEventAsync(newEvent, ct);
                added++;
            }
        }

        // Process explicit deletions from delta
        foreach (var deletedGraphId in deltaResult.DeletedEventIds)
        {
            var externalId = $"outlook_{deletedGraphId}";
            if (existingByExternalId.TryGetValue(externalId, out var eventToDelete))
            {
                await _eventStore.DeleteEventAsync(eventToDelete.Id!, ct);
                deleted++;
            }
        }

        // For full sync (no delta deletions), remove events not seen in response
        if (deltaResult.DeletedEventIds.Count == 0 && deltaResult.AddedOrModified.Count > 0)
        {
            foreach (var existing in existingByExternalId.Values)
            {
                if (!processedExternalIds.Contains(existing.ExternalId!))
                {
                    await _eventStore.DeleteEventAsync(existing.Id!, ct);
                    deleted++;
                }
            }
        }

        return (added, updated, deleted);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _syncLock.Dispose();
    }
}
