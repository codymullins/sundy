using Microsoft.JSInterop;
using Sundy.Core;

namespace Sundy.Services;

/// <summary>
/// Cross-platform notification service implementation.
/// - In Tauri mode: Syncs events to Rust backend for native OS notifications
/// - In browser mode: Uses Web Notifications API
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IJSRuntime _jsRuntime;
    private bool? _isTauri;

    public NotificationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> IsTauriAsync()
    {
        if (_isTauri.HasValue)
        {
            return _isTauri.Value;
        }

        try
        {
            // Use async version that waits for Tauri to initialize (handles race condition in dev mode)
            _isTauri = await _jsRuntime.InvokeAsync<bool>("isTauriEnvironmentAsync");
        }
        catch
        {
            _isTauri = false;
        }

        return _isTauri.Value;
    }

    public async Task<bool> IsSupportedAsync()
    {
        // Tauri always supports notifications via the Rust backend
        if (await IsTauriAsync())
        {
            return true;
        }

        // Check Web Notifications API support
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("webNotifications.isSupported");
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetPermissionAsync()
    {
        // In Tauri mode, check actual OS permission via Rust
        if (await IsTauriAsync())
        {
            try
            {
                var granted = await _jsRuntime.InvokeAsync<bool>("tauriNotifications.checkPermission");
                return granted ? "granted" : "default";
            }
            catch
            {
                return "default";
            }
        }

        try
        {
            return await _jsRuntime.InvokeAsync<string>("webNotifications.getPermission");
        }
        catch
        {
            return "unsupported";
        }
    }

    public async Task<string> RequestPermissionAsync()
    {
        // In Tauri mode, request OS permission via Rust
        if (await IsTauriAsync())
        {
            try
            {
                var granted = await _jsRuntime.InvokeAsync<bool>("tauriNotifications.requestPermission");
                return granted ? "granted" : "denied";
            }
            catch
            {
                return "denied";
            }
        }

        try
        {
            return await _jsRuntime.InvokeAsync<string>("webNotifications.requestPermission");
        }
        catch
        {
            return "unsupported";
        }
    }

    public async Task SyncEventAsync(string id, string title, DateTimeOffset startTime, int reminderMinutes)
    {
        // Only sync to Rust backend in Tauri mode
        if (!await IsTauriAsync())
        {
            return;
        }

        try
        {
            var startTimeUnix = startTime.ToUnixTimeSeconds();
            await _jsRuntime.InvokeAsync<bool>(
                "tauriNotifications.syncEvent",
                id,
                title,
                startTimeUnix,
                reminderMinutes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to sync event to Tauri backend: {ex.Message}");
        }
    }

    public async Task DeleteEventAsync(string id)
    {
        // Only delete from Rust backend in Tauri mode
        if (!await IsTauriAsync())
        {
            return;
        }

        try
        {
            await _jsRuntime.InvokeAsync<bool>("tauriNotifications.deleteEvent", id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete event from Tauri backend: {ex.Message}");
        }
    }

    public async Task SendBrowserNotificationAsync(string title, string body)
    {
        // Only send browser notifications in browser mode
        if (await IsTauriAsync())
        {
            return;
        }

        try
        {
            await _jsRuntime.InvokeVoidAsync("webNotifications.show", title, body, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send browser notification: {ex.Message}");
        }
    }

    public async Task SendReminderNotificationAsync(
        string eventTitle,
        DateTimeOffset eventTime,
        int minutesBefore,
        string eventId)
    {
        // Only send browser notifications in browser mode
        // In Tauri mode, the Rust backend handles this
        if (await IsTauriAsync())
        {
            return;
        }

        try
        {
            // Convert to JavaScript-compatible ISO string
            var eventTimeIso = eventTime.ToString("o");
            await _jsRuntime.InvokeVoidAsync(
                "webNotifications.showReminder",
                eventTitle,
                eventTimeIso,
                minutesBefore,
                eventId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send reminder notification: {ex.Message}");
        }
    }

    public async Task SyncAllUpcomingEventsAsync(IEnumerable<CalendarEvent> events, int defaultReminderMinutes)
    {
        // Only sync to Rust backend in Tauri mode
        if (!await IsTauriAsync())
        {
            return;
        }

        var syncCount = 0;
        foreach (var evt in events)
        {
            if (string.IsNullOrEmpty(evt.Id) || string.IsNullOrEmpty(evt.Title))
            {
                continue;
            }

            // Only sync future events
            if (evt.StartTime <= DateTimeOffset.Now)
            {
                continue;
            }

            try
            {
                await SyncEventAsync(evt.Id, evt.Title, evt.StartTime, defaultReminderMinutes);
                syncCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to sync event {evt.Id}: {ex.Message}");
            }
        }

        if (syncCount > 0)
        {
            Console.WriteLine($"Synced {syncCount} events to notification backend");
        }
    }

    public async Task SendTestNotificationAsync()
    {
        var isTauri = await IsTauriAsync();
        Console.WriteLine($"[DEBUG] SendTestNotificationAsync - IsTauri: {isTauri}");

        if (isTauri)
        {
            // Send test notification via Rust backend
            Console.WriteLine("[DEBUG] Sending via Tauri backend...");
            try
            {
                await _jsRuntime.InvokeAsync<bool>("tauriNotifications.sendTestNotification");
                Console.WriteLine("[DEBUG] Tauri notification sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Failed to send test notification via Tauri: {ex.Message}");
                throw;
            }
        }
        else
        {
            // Send test notification via Web Notifications API
            Console.WriteLine("[DEBUG] Sending via Web Notifications API...");
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    "webNotifications.show",
                    "Test Notification",
                    "Notifications are working!",
                    null);
                Console.WriteLine("[DEBUG] Web notification sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Failed to send test notification via browser: {ex.Message}");
                throw;
            }
        }
    }

    // =========================================================================
    // Persistent Notification Window (Tauri only)
    // =========================================================================

    public async Task<string> GetNotificationPreferenceAsync()
    {
        if (!await IsTauriAsync())
        {
            return "os_only"; // Browser mode only supports OS-style notifications
        }

        try
        {
            return await _jsRuntime.InvokeAsync<string>("tauriNotifications.getNotificationPreference");
        }
        catch
        {
            return "os_only";
        }
    }

    public async Task SetNotificationPreferenceAsync(string preference)
    {
        if (!await IsTauriAsync())
        {
            return;
        }

        try
        {
            await _jsRuntime.InvokeAsync<bool>("tauriNotifications.setNotificationPreference", preference);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to set notification preference: {ex.Message}");
        }
    }

    public async Task ShowNotificationWindowAsync()
    {
        if (!await IsTauriAsync())
        {
            return;
        }

        try
        {
            await _jsRuntime.InvokeAsync<bool>("tauriNotifications.showNotificationWindow");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to show notification window: {ex.Message}");
        }
    }

    public async Task HideNotificationWindowAsync()
    {
        if (!await IsTauriAsync())
        {
            return;
        }

        try
        {
            await _jsRuntime.InvokeAsync<bool>("tauriNotifications.hideNotificationWindow");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to hide notification window: {ex.Message}");
        }
    }

    public async Task<string?> AddPersistentNotificationAsync(string eventId, string title, string body)
    {
        if (!await IsTauriAsync())
        {
            return null;
        }

        try
        {
            return await _jsRuntime.InvokeAsync<string?>(
                "tauriNotifications.addPersistentNotification",
                eventId,
                title,
                body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to add persistent notification: {ex.Message}");
            return null;
        }
    }

    public async Task DismissPersistentNotificationAsync(string notificationId)
    {
        if (!await IsTauriAsync())
        {
            return;
        }

        try
        {
            await _jsRuntime.InvokeAsync<bool>(
                "tauriNotifications.dismissPersistentNotification",
                notificationId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to dismiss persistent notification: {ex.Message}");
        }
    }
}
