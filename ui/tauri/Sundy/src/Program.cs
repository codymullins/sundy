using System.Data;
using Mediator;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using SqliteWasmBlazor;
using Sundy;
using Sundy.Core;
using Sundy.Core.Calendars.Outlook;
using Sundy.Core.Calendars.Sync;
using Sundy.Core.Commands;
using Sundy.Core.Meta;
using Sundy.Core.Settings;
using Sundy.Core.Sync;
using Sundy.Core.Backup;
using Sundy.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.UseSentry(options =>
{
    options.Dsn = "https://9f30a52c8bf1f541d19608f1a72bbd7a@o4510624915062784.ingest.us.sentry.io/4510624917291008";
    options.Debug = false;
    options.SendDefaultPii = false;
    options.TracesSampleRate = 0.1; // 10% sampling for production

    // Filter events based on telemetry opt-in setting
    options.SetBeforeSend((sentryEvent, hint) => !TelemetryConfig.IsEnabled ? null : // Drop the event
        sentryEvent);

    // Also filter transactions
    options.SetBeforeSendTransaction((transaction, hint) => !TelemetryConfig.IsEnabled ? null : transaction);
});
// Captures logError and higher as events
builder.Logging.AddSentry(o => o.InitializeSdk = false);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register OPFS-backed SQLite connection for persistent storage
var connection = new SqliteWasmConnection("Data Source=Sundy.db");
builder.Services.AddSingleton<IDbConnection>(connection);

// Register database manager and Dapper-based stores
builder.Services.AddSingleton<DapperDatabaseManager>();
builder.Services.AddSingleton<IEventStore, DapperEventStore>();
builder.Services.AddSingleton<ICalendarStore, DapperCalendarStore>();

// Register sync infrastructure
builder.Services.AddSingleton<ISyncMetadataStore, DapperSyncMetadataStore>();
builder.Services.AddSingleton<IUploadQueueStore, DapperUploadQueueStore>();
builder.Services.AddSingleton<OperationRecorder>();
builder.Services.AddSingleton<ISyncService>(sp =>
{
    var httpClient = new HttpClient(); // Separate HttpClient for sync API
    return new SyncService(
        httpClient,
        sp.GetRequiredService<ISyncMetadataStore>(),
        sp.GetRequiredService<IUploadQueueStore>(),
        sp.GetRequiredService<ICalendarStore>(),
        sp.GetRequiredService<IEventStore>());
});

// Register Outlook multi-account support (WASM-compatible HTTP-based auth)
builder.Services.AddSingleton<OutlookGraphOptions>(_ => new OutlookGraphOptions
{
    UseDeviceCodeFlow = true
});
builder.Services.AddSingleton<IConnectedAccountStore, DapperConnectedAccountStore>();

// Register WASM auth provider (handles OAuth popup flow via JS interop)
builder.Services.AddSingleton<IMicrosoftAuthProvider>(sp =>
    new WasmMicrosoftAuthProvider(
        sp.GetRequiredService<IConnectedAccountStore>(),
        sp.GetRequiredService<IJSRuntime>(),
        sp.GetRequiredService<OutlookGraphOptions>()));

// Register platform-agnostic account manager (uses auth provider)
builder.Services.AddSingleton<ILogger<MicrosoftAccountManager>>(
    _ => NullLogger<MicrosoftAccountManager>.Instance);
builder.Services.AddSingleton<IMicrosoftAccountManager>(sp =>
    new MicrosoftAccountManager(
        sp.GetRequiredService<ILogger<MicrosoftAccountManager>>(),
        sp.GetRequiredService<IConnectedAccountStore>(),
        sp.GetRequiredService<IMicrosoftAuthProvider>()));
builder.Services.AddSingleton<OutlookCalendarProvider>();

// Register sync state management (for UI reactivity)
builder.Services.AddSingleton<ISyncStateManager, SyncStateManager>();
builder.Services.AddSingleton<IToastService, ToastService>();
builder.Services.AddSingleton<ISyncDeltaStore, DapperSyncDeltaStore>();
builder.Services.AddSingleton<ISettingsService, DapperSettingsService>();

// Register backup service
builder.Services.AddSingleton<ILogger<DapperBackupService>>(
    _ => NullLogger<DapperBackupService>.Instance);
builder.Services.AddSingleton<IBackupService>(sp =>
    new DapperBackupService(
        sp.GetRequiredService<IDbConnection>(),
        sp.GetRequiredService<ICalendarStore>(),
        sp.GetRequiredService<IConnectedAccountStore>(),
        sp.GetRequiredService<ILogger<DapperBackupService>>()));
builder.Services.AddSingleton<ILogger<BackupScheduler>>(
    _ => NullLogger<BackupScheduler>.Instance);
builder.Services.AddSingleton<BackupScheduler>();

// Register background Outlook sync service
builder.Services.AddSingleton<ILogger<OutlookSyncService>>(
    _ => NullLogger<OutlookSyncService>.Instance);
builder.Services.AddSingleton<IOutlookSyncService>(sp =>
    new OutlookSyncService(
        sp.GetRequiredService<OutlookCalendarProvider>(),
        sp.GetRequiredService<ICalendarStore>(),
        sp.GetRequiredService<IEventStore>(),
        sp.GetRequiredService<ISyncStateManager>(),
        sp.GetRequiredService<ISyncDeltaStore>(),
        sp.GetRequiredService<IConnectedAccountStore>(),
        sp.GetRequiredService<ISettingsService>(),
        sp.GetRequiredService<ILogger<OutlookSyncService>>()));

// Register Mediator
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Singleton;
});

var host = builder.Build();

// Initialize telemetry setting from localStorage (before any errors could be sent)
var jsRuntime = host.Services.GetRequiredService<IJSRuntime>();
await TelemetryConfig.InitializeAsync(jsRuntime);

// Open the OPFS-backed connection (must be done before any DB operations)
await connection.OpenAsync();

// Initialize database schema
var mediator = host.Services.GetRequiredService<IMediator>();
await mediator.Send(new InitializeDatabaseCommand());

// Initialize the Microsoft account manager (loads accounts from store)
var accountManager = host.Services.GetRequiredService<IMicrosoftAccountManager>();
await accountManager.InitializeAsync();

// Seed initial data
await SeedDataAsync(host.Services);

// Apply demo mode settings from configuration
await ApplyDemoModeSettingsAsync(host.Services, host.Configuration);

// Start the background Outlook sync service
var outlookSyncService = host.Services.GetRequiredService<IOutlookSyncService>();
await outlookSyncService.StartAsync();

// Start the backup scheduler (if automatic backups are enabled)
var backupScheduler = host.Services.GetRequiredService<BackupScheduler>();
await backupScheduler.StartAsync();

await host.RunAsync();

static async Task SeedDataAsync(IServiceProvider services)
{
    var mediator = services.GetRequiredService<IMediator>();
    var calendarStore = services.GetRequiredService<ICalendarStore>();

    // Check if data already exists
    var existingCalendars = await calendarStore.GetAllAsync(CancellationToken.None);
    if (existingCalendars.Count > 0) return;

    // Seed default calendar
    var myCalendar = new Calendar
    {
        Id = Guid.NewGuid().ToString(),
        Name = "My Calendar",
        Color = "#4285f4",
        Type = CalendarType.Local
    };
    await mediator.Send(new CreateCalendarCommand(myCalendar));
}

static async Task ApplyDemoModeSettingsAsync(IServiceProvider services, IConfiguration configuration)
{
    var demoMode = configuration.GetValue<bool>("DemoMode");
    if (!demoMode) return;

    var settingsService = services.GetRequiredService<ISettingsService>();

    // Set demo mode flag
    await settingsService.SetDemoModeAsync(true);

    // In demo mode, enable telemetry by default (user can still disable it)
    // Only set if not already configured by user
    var telemetryValue = await settingsService.GetAsync(SettingKeys.TelemetryEnabled);
    if (telemetryValue == null)
    {
        await settingsService.SetTelemetryEnabledAsync(true);

        // Also update localStorage and TelemetryConfig for immediate effect
        var jsRuntime = services.GetRequiredService<IJSRuntime>();
        await jsRuntime.InvokeVoidAsync("telemetryHelper.setEnabled", true);
        TelemetryConfig.SetEnabled(true);
    }
}

/// <summary>
/// Static configuration for telemetry that can be read before services are built.
/// Uses localStorage via JS interop for persistence.
/// </summary>
public static class TelemetryConfig
{
    private static bool _isEnabled;
    private static bool _initialized;

    public static bool IsEnabled => _isEnabled;

    /// <summary>
    /// Initialize from localStorage via JS interop (called early in startup).
    /// </summary>
    public static async Task InitializeAsync(IJSRuntime jsRuntime)
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            _isEnabled = await jsRuntime.InvokeAsync<bool>("telemetryHelper.getEnabled");
        }
        catch
        {
            _isEnabled = false; // Default to disabled on error
        }
        _initialized = true;
    }

    /// <summary>
    /// Update the setting (called when user changes preference).
    /// </summary>
    public static void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
    }
}
