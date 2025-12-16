using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Mediator;
using Sundy.Core;
using Sundy.Core.Calendars.Outlook;
using Sundy.Core.System;
using Sundy.Uno.ViewModels;
using Uno.Resizetizer;

namespace Sundy.Uno;

public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    public IHost? Host { get; private set; }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            // Add navigation support for toolkit controls such as TabBar and NavigationView
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    // Configure log levels for different categories of logging
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ? LogLevel.Information : LogLevel.Warning)

                        // Default filters for core Uno Platform namespaces
                        .CoreLogLevel(LogLevel.Warning);

                    // Uno Platform namespace filter groups
                    // Uncomment individual methods to see more detailed logging
                    //// Generic Xaml events
                    //logBuilder.XamlLogLevel(LogLevel.Debug);
                    //// Layout specific messages
                    //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                    //// Storage messages
                    //logBuilder.StorageLogLevel(LogLevel.Debug);
                    //// Binding related messages
                    //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                    //// Binder memory references tracking
                    //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                    //// DevServer and HotReload related
                    //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                    //// Debug JS interop
                    //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);
                }, enableUnoLogging: true)
                .UseSerilog(consoleLoggingEnabled: true, fileLoggingEnabled: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                .UseLocalization()
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging();
                    services.AddMediator(options => { options.ServiceLifetime = ServiceLifetime.Scoped; });

                    var dbPath = ResolveDatabasePath();
                    var connectionString = $"Data Source={dbPath}";
                    services.AddDbContext<SundyDbContext>(options => options.UseSqlite(connectionString));

                    services.AddScoped<EventStore>();
                    services.AddScoped<DatabaseManager>();
                    services.AddScoped<CalendarStore>();

                    services.AddSingleton<ICalendarProvider, LocalCalendarProvider>();

                    // Time zone provider (required by EventTimeService)
                    services.AddSingleton<ITimeZoneProvider, SystemTimeZoneProvider>();

                    // Event time service (required by MainViewModel)
                    services.AddSingleton<EventTimeService>();

                    // Outlook integration services
                    services.AddSingleton<OutlookGraphOptions>(sp => new OutlookGraphOptions
                    {
                        // MacOS 25.1+ doesn't support InteractiveBrowserCredential
                        UseDevelopmentCredential = OperatingSystem.IsMacOS()
                    });
                    services.AddSingleton<MicrosoftGraphAuthService>();
                    services.AddSingleton<OutlookCalendarProvider>();

                    services.AddTransient<Presentation.ShellViewModel>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<CalendarViewModel>();
                    services.AddTransient<CalendarSettingsViewModel>();
                    services.AddTransient<EventEditViewModel>();
                })
                .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();

        if (Host is not null)
        {
            using var scope = Host.Services.CreateScope();
            var metaStore = scope.ServiceProvider.GetRequiredService<DatabaseManager>();
            await metaStore.InitializeDatabaseAsync();
        }
    }

    private static string ResolveDatabasePath()
    {
        if (OperatingSystem.IsBrowser())
        {
            return "file::memory:?cache=shared";
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var sundyDataPath = Path.Combine(appDataPath, "Sundy");
        Directory.CreateDirectory(sundyDataPath);
        return Path.Combine(sundyDataPath, "sundy.db");
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(Presentation.ShellViewModel)),
            new ViewMap<MainPage, ViewModels.MainViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<Presentation.ShellViewModel>(),
                Nested:
                [
                    new("Main", View: views.FindByViewModel<ViewModels.MainViewModel>(), IsDefault: true)
                ]
            )
        );
    }
}
