using Sundy.Core;
using Sundy.Core.Settings;

namespace Sundy.Services;

/// <summary>
/// Background service for scheduling and triggering calendar event reminders.
/// - In Tauri mode: Events are synced to Rust backend via INotificationService
/// - In browser mode: Polls the event store and sends Web Notifications
/// </summary>
public class ReminderScheduler : IReminderScheduler, IAsyncDisposable
{
    private readonly IEventStore _eventStore;
    private readonly ISettingsService _settingsService;
    private readonly INotificationService _notificationService;

    private Timer? _checkTimer;
    private CancellationTokenSource? _cts;
    private readonly HashSet<string> _sentReminders = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    // Check for reminders every 30 seconds
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);
    // Look ahead window for upcoming events (2 hours)
    private static readonly TimeSpan LookAheadWindow = TimeSpan.FromHours(2);

    public ReminderScheduler(
        IEventStore eventStore,
        ISettingsService settingsService,
        INotificationService notificationService)
    {
        _eventStore = eventStore;
        _settingsService = settingsService;
        _notificationService = notificationService;
    }

    public bool IsRunning => _checkTimer != null;

    public async Task StartAsync(CancellationToken ct = default)
    {
        // In Tauri mode, the Rust backend handles reminder scheduling
        // We only start the browser-based scheduler in browser mode
        if (await _notificationService.IsTauriAsync())
        {
            Console.WriteLine("Running in Tauri mode - reminders handled by Rust backend");
            return;
        }

        if (_checkTimer != null)
        {
            return;
        }

        // Check if reminders are enabled
        var enabled = await GetRemindersEnabledAsync(ct);
        if (!enabled)
        {
            Console.WriteLine("Reminders are disabled");
            return;
        }

        // Check notification permission
        var permission = await _notificationService.GetPermissionAsync();
        if (permission != "granted")
        {
            Console.WriteLine($"Notification permission not granted: {permission}");
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // Start periodic check timer
        _checkTimer = new Timer(
            async _ => await CheckRemindersInternalAsync(),
            null,
            TimeSpan.FromSeconds(5), // Initial delay
            CheckInterval);

        Console.WriteLine("Browser reminder scheduler started");
    }

    public async Task StopAsync()
    {
        if (_checkTimer != null)
        {
            await _checkTimer.DisposeAsync();
            _checkTimer = null;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _sentReminders.Clear();
        Console.WriteLine("Reminder scheduler stopped");
    }

    public async Task CheckRemindersAsync(CancellationToken ct = default)
    {
        // Skip if in Tauri mode
        if (await _notificationService.IsTauriAsync())
        {
            return;
        }

        await CheckRemindersInternalAsync();
    }

    private async Task CheckRemindersInternalAsync()
    {
        if (!await _lock.WaitAsync(0))
        {
            return; // Already checking
        }

        try
        {
            var now = DateTimeOffset.Now;
            var lookAheadEnd = now.Add(LookAheadWindow);

            // Get default reminder time
            var reminderMinutes = await GetDefaultReminderMinutesAsync();

            // Get upcoming events
            var events = await _eventStore.GetEventsInRangeAsync(now, lookAheadEnd);

            // Clean up old sent reminders (events that have started)
            var expiredKeys = _sentReminders
                .Where(id => events.All(e => e.Id != id || e.StartTime <= now))
                .ToList();
            foreach (var key in expiredKeys)
            {
                _sentReminders.Remove(key);
            }

            // Check each event for due reminders
            foreach (var evt in events)
            {
                if (string.IsNullOrEmpty(evt.Id) || string.IsNullOrEmpty(evt.Title))
                {
                    continue;
                }

                // Skip if already sent
                var reminderKey = $"{evt.Id}_{reminderMinutes}";
                if (_sentReminders.Contains(reminderKey))
                {
                    continue;
                }

                // Calculate when the reminder should fire
                var reminderTime = evt.StartTime.AddMinutes(-reminderMinutes);

                // If reminder time has passed and event started within last 5 minutes
                // (5-minute grace period allows "at start time" reminders to work)
                if (reminderTime <= now && evt.StartTime > now.AddMinutes(-5))
                {
                    // Calculate actual minutes until event
                    var minutesUntil = (int)Math.Ceiling((evt.StartTime - now).TotalMinutes);

                    await _notificationService.SendReminderNotificationAsync(
                        evt.Title,
                        evt.StartTime,
                        minutesUntil,
                        evt.Id);

                    _sentReminders.Add(reminderKey);
                    Console.WriteLine($"Sent reminder for event: {evt.Title} ({evt.Id})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking reminders: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<bool> GetRemindersEnabledAsync(CancellationToken ct = default)
    {
        try
        {
            var value = await _settingsService.GetAsync(SettingKeys.RemindersEnabled, ct);
            // Default to true if not set
            return value == null || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return true; // Default to enabled
        }
    }

    private async Task<int> GetDefaultReminderMinutesAsync(CancellationToken ct = default)
    {
        try
        {
            var value = await _settingsService.GetAsync(SettingKeys.DefaultReminderMinutes, ct);
            if (int.TryParse(value, out var minutes) && minutes > 0)
            {
                return minutes;
            }
        }
        catch
        {
            // Ignore errors
        }

        return 15; // Default to 15 minutes
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _lock.Dispose();
    }
}
