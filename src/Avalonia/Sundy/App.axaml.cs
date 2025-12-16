using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sundy.Core;
using Sundy.Core.Calendars.Outlook;
using Sundy.Core.System;
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
        services.AddLogging(builder => { builder.AddSerilog(dispose: false); });

        // Add Mediator
        services.AddMediator(options => { options.ServiceLifetime = ServiceLifetime.Scoped; });

        // Get the application data path
        string dbPath;
        if (OperatingSystem.IsBrowser())
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

        // Register stores and database manager
        services.AddScoped<EventStore>();
        services.AddScoped<DatabaseManager>();
        services.AddScoped<CalendarStore>();

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

        // Initialize database schema
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SundyDbContext>();
            var metaStore = scope.ServiceProvider.GetRequiredService<DatabaseManager>();

            if (!metaStore.DatabaseExistsAsync(CancellationToken.None).GetAwaiter().GetResult())
            {
                Log.Information("Database not found. Creating new database at {DbPath}", dbPath);
            }
            metaStore.InitializeDatabaseAsync().GetAwaiter().GetResult();
        }

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

            // Initialize the view model asynchronously
            _ = mainViewModel.InitializeAsync();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // Get MainViewModel from DI
            var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();

            singleViewPlatform.MainView = new MainView
            {
                DataContext = mainViewModel
            };

            // Initialize the view model asynchronously
            _ = mainViewModel.InitializeAsync();
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
