using System.Data;
using System.Text.Json;
using Dapper;

namespace Sundy.Core.Settings;

/// <summary>
/// Dapper implementation of ISettingsService using SQLite.
/// Uses the existing Settings table created by DapperDatabaseManager.
/// </summary>
public class DapperSettingsService(IDbConnection connection) : ISettingsService
{
    private const int DefaultSyncIntervalMinutes = 5;
    private const bool DefaultShowStatusBar = false;

    public event Action<string>? OnSettingChanged;

    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        const string sql = "SELECT Value FROM Settings WHERE Key = @Key AND IsDeleted = 0";
        return await connection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(sql, new { Key = key }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await GetAsync(key, ct).ConfigureAwait(false);
        if (value == null) return default;

        try
        {
            // Handle primitive types directly
            var targetType = typeof(T);
            if (targetType == typeof(int)) return (T)(object)int.Parse(value);
            if (targetType == typeof(bool)) return (T)(object)bool.Parse(value);
            if (targetType == typeof(string)) return (T)(object)value;

            // Complex types use JSON
            return JsonSerializer.Deserialize<T>(value);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync(string key, string value, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow.ToString("o");
        const string sql = """
            INSERT INTO Settings (Key, Value, Version, UpdatedAt, IsDeleted)
            VALUES (@Key, @Value, 0, @UpdatedAt, 0)
            ON CONFLICT(Key) DO UPDATE SET
                Value = @Value,
                UpdatedAt = @UpdatedAt,
                IsDeleted = 0
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                Key = key,
                Value = value,
                UpdatedAt = now
            }, cancellationToken: ct)).ConfigureAwait(false);

        OnSettingChanged?.Invoke(key);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken ct = default)
    {
        string stringValue;
        var valueType = typeof(T);

        // Handle primitive types directly
        if (valueType == typeof(int) || valueType == typeof(bool) || valueType == typeof(string))
        {
            stringValue = value?.ToString() ?? "";
        }
        else
        {
            // Complex types use JSON
            stringValue = JsonSerializer.Serialize(value);
        }

        await SetAsync(key, stringValue, ct).ConfigureAwait(false);
    }

    public async Task<int> GetSyncIntervalMinutesAsync(CancellationToken ct = default)
    {
        var value = await GetAsync<int>(SettingKeys.SyncIntervalMinutes, ct).ConfigureAwait(false);
        return value > 0 ? value : DefaultSyncIntervalMinutes;
    }

    public async Task SetSyncIntervalMinutesAsync(int minutes, CancellationToken ct = default)
    {
        // Validate valid intervals
        if (minutes is not (1 or 5 or 15 or 30))
        {
            minutes = DefaultSyncIntervalMinutes;
        }

        await SetAsync(SettingKeys.SyncIntervalMinutes, minutes, ct).ConfigureAwait(false);
    }

    public async Task<bool> GetShowStatusBarAsync(CancellationToken ct = default)
    {
        var value = await GetAsync(SettingKeys.ShowStatusBar, ct).ConfigureAwait(false);
        return value?.ToLowerInvariant() == "true";
    }

    public async Task SetShowStatusBarAsync(bool show, CancellationToken ct = default)
    {
        await SetAsync(SettingKeys.ShowStatusBar, show.ToString(), ct).ConfigureAwait(false);
    }

    public async Task<bool> GetPrivacyModeAsync(CancellationToken ct = default)
    {
        var value = await GetAsync(SettingKeys.PrivacyMode, ct).ConfigureAwait(false);
        return value?.ToLowerInvariant() == "true";
    }

    public async Task SetPrivacyModeAsync(bool enabled, CancellationToken ct = default)
    {
        await SetAsync(SettingKeys.PrivacyMode, enabled.ToString(), ct).ConfigureAwait(false);
    }

    public async Task<bool> GetPrivacyHideEmailsAsync(CancellationToken ct = default)
    {
        var value = await GetAsync(SettingKeys.PrivacyHideEmails, ct).ConfigureAwait(false);
        // Default to true (enabled) when not set
        return value == null || value.ToLowerInvariant() == "true";
    }

    public async Task SetPrivacyHideEmailsAsync(bool enabled, CancellationToken ct = default)
    {
        await SetAsync(SettingKeys.PrivacyHideEmails, enabled.ToString(), ct).ConfigureAwait(false);
    }

    public async Task<bool> GetPrivacyHideEventTitlesAsync(CancellationToken ct = default)
    {
        var value = await GetAsync(SettingKeys.PrivacyHideEventTitles, ct).ConfigureAwait(false);
        // Default to false (disabled) when not set
        return value?.ToLowerInvariant() == "true";
    }

    public async Task SetPrivacyHideEventTitlesAsync(bool enabled, CancellationToken ct = default)
    {
        await SetAsync(SettingKeys.PrivacyHideEventTitles, enabled.ToString(), ct).ConfigureAwait(false);
    }

    public async Task<bool> GetCollapsePastEventsAsync(CancellationToken ct = default)
    {
        var value = await GetAsync(SettingKeys.CollapsePastEvents, ct).ConfigureAwait(false);
        // Default to true (enabled) if not set
        return value == null || value.ToLowerInvariant() == "true";
    }

    public async Task SetCollapsePastEventsAsync(bool enabled, CancellationToken ct = default)
    {
        await SetAsync(SettingKeys.CollapsePastEvents, enabled.ToString(), ct).ConfigureAwait(false);
    }

    public async Task<bool> GetDynamicViewEnabledAsync(CancellationToken ct = default)
    {
        var value = await GetAsync(SettingKeys.DynamicViewEnabled, ct).ConfigureAwait(false);
        // Default to false (disabled) - feature flag
        return value?.ToLowerInvariant() == "true";
    }

    public async Task SetDynamicViewEnabledAsync(bool enabled, CancellationToken ct = default)
    {
        await SetAsync(SettingKeys.DynamicViewEnabled, enabled.ToString(), ct).ConfigureAwait(false);
    }

    public async Task<bool> GetTelemetryEnabledAsync(CancellationToken ct = default)
    {
        var value = await GetAsync(SettingKeys.TelemetryEnabled, ct).ConfigureAwait(false);
        // Default to false (opt-in, not opt-out)
        return value?.ToLowerInvariant() == "true";
    }

    public async Task SetTelemetryEnabledAsync(bool enabled, CancellationToken ct = default)
    {
        await SetAsync(SettingKeys.TelemetryEnabled, enabled.ToString(), ct).ConfigureAwait(false);
    }

    public async Task<bool> GetDemoModeAsync(CancellationToken ct = default)
    {
        var value = await GetAsync(SettingKeys.DemoMode, ct).ConfigureAwait(false);
        return value?.ToLowerInvariant() == "true";
    }

    public async Task SetDemoModeAsync(bool enabled, CancellationToken ct = default)
    {
        await SetAsync(SettingKeys.DemoMode, enabled.ToString(), ct).ConfigureAwait(false);
    }

    public async Task<bool> GetDemoBannerDismissedAsync(CancellationToken ct = default)
    {
        var value = await GetAsync(SettingKeys.DemoBannerDismissed, ct).ConfigureAwait(false);
        return value?.ToLowerInvariant() == "true";
    }

    public async Task SetDemoBannerDismissedAsync(bool dismissed, CancellationToken ct = default)
    {
        await SetAsync(SettingKeys.DemoBannerDismissed, dismissed.ToString(), ct).ConfigureAwait(false);
    }
}
