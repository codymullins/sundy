using System.Data;
using Serilog;
using Sundy.Core;
using Sundy.Core.Calendars.Outlook;
using Sundy.Core.Meta;
using Sundy.Uno.Services;
using Sundy.Uno.ViewModels;

namespace Sundy.Uno;

/// <summary>
/// Uno Platform Application with Dependency Injection.
/// Migrated from Avalonia App.axaml.cs - preserves same DI container setup.
/// Doc reference: https://platform.uno/docs/articles/getting-started/wizard/includes/extensions.html#dependency-injection
/// </summary>
public partial class App : Application
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }

    public static IServiceProvider Services =>
        _serviceProvider ?? throw new InvalidOperationException("ServiceProvider not initialized");

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            Console.WriteLine("[App] OnLaunched starting...");
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Console.WriteLine($"[App] UnhandledException: {e.ExceptionObject}");
            TaskScheduler.UnobservedTaskException += (s, e) =>
                Console.WriteLine($"[App] UnobservedTaskException: {e.Exception}");

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            // Configure Dependency Injection
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder => { builder.AddSerilog(dispose: false); });

            // Add Mediator (CQRS pattern)
            services.AddMediator(options => { options.ServiceLifetime = ServiceLifetime.Scoped; });

            // Get the application data path
            string dbPath;
#if __WASM__
        // For browser, use in-memory database with shared cache
        dbPath = "file::memory:?cache=shared";
#else
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var sundyDataPath = Path.Combine(appDataPath, "Sundy");
            Directory.CreateDirectory(sundyDataPath);
            dbPath = Path.Combine(sundyDataPath, "sundy.db");
#endif

            // Register DbContext
            var connectionString = $"Data Source={dbPath}";
            // services.AddDbContext<SundyDbContext>(options =>
            //     options.UseSqlite(connectionString));

            // Register stores and database manager
            services.AddScoped<IEventStore, DapperEventStore>();
            services.AddScoped<ICalendarStore, DapperCalendarStore>();
            services.AddScoped<DapperDatabaseManager>();
            services.AddScoped<IDbConnection>(_ =>
            {
                var conn = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
                conn.Open();
                return conn;
            });
            // Register Services
            services.AddSingleton<ICalendarProvider, LocalCalendarProvider>();
            services.AddSingleton<ITimeZoneProvider, SystemTimeZoneProvider>();
            services.AddSingleton<EventTimeService>();

            // Register Outlook/Microsoft Graph services
            services.AddSingleton(new OutlookGraphOptions
            {
                UseDevelopmentCredential = false,
                UseDeviceCodeFlow = false
            });

            // Register auth service
            services.AddSingleton<MicrosoftGraphAuthService>();
            services.AddSingleton<IMicrosoftGraphAuthService>(sp => sp.GetRequiredService<MicrosoftGraphAuthService>());
            services.AddSingleton<OutlookCalendarProvider>();

            // Enable copying to clipboard
            services.AddSingleton<IClipboardService, ClipboardService>();

            // XamlRoot provider for ContentDialog support
            services.AddSingleton<IXamlRootProvider, XamlRootProvider>();

            // Menu bar service for macOS status bar item
            services.AddSingleton<IMenuBarService, MenuBarService>();

            // Register ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<CalendarViewModel>();
            services.AddTransient<CalendarSettingsViewModel>();
            services.AddTransient<EventEditViewModel>();

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // Initialize database schema
            // using (var scope = _serviceProvider.CreateScope())
            // {
            //     var dbContext = scope.ServiceProvider.GetRequiredService<SundyDbContext>();
            //     var metaStore = scope.ServiceProvider.GetRequiredService<DatabaseManager>();
            //
            //     if (!metaStore.DatabaseExistsAsync(CancellationToken.None).GetAwaiter().GetResult())
            //     {
            //         Log.Information("Database not found. Creating new database at {DbPath}", dbPath);
            //     }
            //     metaStore.InitializeDatabaseAsync().GetAwaiter().GetResult();
            // }

            MainWindow = new Window();
// #if DEBUG
//         MainWindow.UseStudio();
// #endif

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (MainWindow.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Place the frame in the current Window
                MainWindow.Content = rootFrame;

                rootFrame.NavigationFailed += OnNavigationFailed;
            }

            if (rootFrame.Content == null)
            {
                // Navigate to MainPage
                rootFrame.Navigate(typeof(MainPage), args.Arguments);
            }

            MainWindow.SetWindowIcon();
            // Ensure the current window is active
            Console.WriteLine("[App] Activating main window...");
            MainWindow.Activate();
            Console.WriteLine("[App] Main window activated");

            // Initialize macOS menu bar (status bar item with mini calendar)
            // Dispatch to run after the main loop stabilizes
            Console.WriteLine("[App] Scheduling menu bar initialization...");
            if (!MainWindow.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
            {
                Console.WriteLine("[App] Dispatched: Initializing menu bar...");
                InitializeMenuBar(rootFrame);
                Console.WriteLine("[App] Dispatched: Menu bar initialized!");
            }))
            {
                Console.WriteLine("[App] WARNING: Failed to enqueue menu bar initialization!");
            }
            else
            {
                Console.WriteLine("[App] Menu bar initialization enqueued successfully");
            };
            Console.WriteLine("[App] OnLaunched complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[App] OnLaunched EXCEPTION: {ex}");
            throw;
        }
    }

    private void InitializeMenuBar(Frame rootFrame)
    {
        if (!OperatingSystem.IsMacOS()) return;

        var menuBarService = _serviceProvider?.GetService<IMenuBarService>();
        if (menuBarService == null) return;

        menuBarService.Initialize();

        // Handle date clicks from the mini calendar
        menuBarService.DateClicked += (date) =>
        {
            // Bring window to front
            MainWindow?.Activate();

            // Navigate to the clicked date via the MainViewModel
            if (rootFrame.Content is MainPage mainPage)
            {
                mainPage.ViewModel?.NavigateToDate(date);
            }
        };
    }

    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
    }

    /// <summary>
    /// Configures global Uno Platform logging
    /// </summary>
    public static void InitializeLogging()
    {
#if DEBUG
        // Logging is disabled by default for release builds, as it incurs a significant
        // initialization cost from Microsoft.Extensions.Logging setup. If startup performance
        // is a concern for your application, keep this disabled. If you're running on the web or
        // desktop targets, you can use URL or command line parameters to enable it.
        //
        // For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

        var factory = LoggerFactory.Create(builder =>
        {
#if __WASM__
            builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
            builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());

            // Log to the Visual Studio Debug console
            builder.AddConsole();
#else
            builder.AddConsole();
#endif

            // Exclude logs below this level
            builder.SetMinimumLevel(LogLevel.Information);

            // Default filters for Uno Platform namespaces
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);

            // Generic Xaml events
            // builder.AddFilter("Microsoft.UI.Xaml", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.VisualStateGroup", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.StateTriggerBase", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.UIElement", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.FrameworkElement", LogLevel.Trace );

            // Layouter specific messages
            // builder.AddFilter("Microsoft.UI.Xaml.Controls", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Controls.Layouter", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Controls.Panel", LogLevel.Debug );

            // builder.AddFilter("Windows.Storage", LogLevel.Debug );

            // Binding related messages
            // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );

            // Binder memory references tracking
            // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

            // DevServer and HotReload related
            // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

            // Debug JS interop
            // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
    }
}
