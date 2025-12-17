using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sundy.Core;
using Sundy.Core.Calendars.Outlook;
using Sundy.Core.Meta;
using Sundy.Services;
using Sundy.ViewModels;
using Sundy.Views;

namespace Sundy;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => { });

        // Add Mediator
        services.AddMediator(options => { options.ServiceLifetime = ServiceLifetime.Scoped; });

        // Get the application data path
        string dbPath;
        if (OperatingSystem.IsBrowser() || OperatingSystem.IsIOS())
        {
            // For browser, use in-memory database with shared cache
            dbPath = "file::memory:?cache=shared";
        }
        else
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var sundyDataPath = Path.Combine(appDataPath, "Sundy");
            Directory.CreateDirectory(sundyDataPath);
            dbPath = Path.Combine(sundyDataPath, "sundy.db");
        }

        // Register DbContext
        var connectionString = $"Data Source={dbPath}";
        services.AddDbContext<SundyDbContext>(options =>
            options.UseSqlite(connectionString));

        // Register IDbConnection for Dapper stores
        services.AddScoped<System.Data.IDbConnection>(_ =>
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            connection.Open();
            return connection;
        });

        // Register stores and database manager
        services.AddScoped<DatabaseManager>();
        services.AddScoped<DapperDatabaseManager>();

        if (OperatingSystem.IsIOS())
        {
            services.AddScoped<IEventStore, DapperEventStore>();
            services.AddScoped<ICalendarStore, DapperCalendarStore>();
            // services.AddScoped<IEventStore, InMemoryEventStore>();
            // services.AddScoped<ICalendarStore, InMemoryCalendarStore>();
        }
        else
        {
            services.AddScoped<IEventStore, DapperEventStore>();
            services.AddScoped<ICalendarStore, DapperCalendarStore>();
            // services.AddScoped<IEventStore, SQLiteEventStore>();
            // services.AddScoped<ICalendarStore, SQLiteCalendarStore>();
        }

        // Register Services
        services.AddSingleton<ICalendarProvider, LocalCalendarProvider>();
        services.AddSingleton<ITimeZoneProvider, SystemTimeZoneProvider>();
        services.AddSingleton<EventTimeService>();

        // Register Outlook/Microsoft Graph services
        services.AddSingleton(new OutlookGraphOptions
        {
            UseDevelopmentCredential = false,
            UseDeviceCodeFlow = false  // Enable device code flow
        });
        
        // Register auth service - browser platform will override this registration
        // in the Sundy.Browser project with BrowserMicrosoftGraphAuthService
        services.AddSingleton<MicrosoftGraphAuthService>();
        services.AddSingleton<IMicrosoftGraphAuthService>(sp => sp.GetRequiredService<MicrosoftGraphAuthService>());
        services.AddSingleton<OutlookCalendarProvider>();

        // enable copying to clipboard
        services.AddSingleton<Services.IClipboardService, Services.ClipboardService>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<CalendarViewModel>();
        services.AddTransient<CalendarSettingsViewModel>();
        services.AddTransient<EventEditViewModel>();

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Get MainViewModel from DI
            var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            
            Dispatcher.UIThread.Post(async void () =>
            {
                try
                {
                    await mainViewModel.InitializeAsync();
                }
                catch (Exception ex)
                {
                    // log
                    // Log.Fatal(ex, "Failed to initialize MainViewModel: {Message}", ex.Message);
                }
            });
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // Get MainViewModel from DI
            var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();

            singleViewPlatform.MainView = new MainView
            {
                DataContext = mainViewModel
            };

            Dispatcher.UIThread.Post(async void () =>
            {
                try
                {
                    await mainViewModel.InitializeAsync();
                }
                catch (Exception ex)
                {
                    // log
                    // Log.Fatal(ex, "Failed to initialize MainViewModel: {Message}", ex.Message);
                }
            });
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
