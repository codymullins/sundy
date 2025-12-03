using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sundy.Core;
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

        // todo: revisit this setup later
        var connectionString = $"Data Source={dbPath}";
        services.AddSingleton<EventStore>(_ =>
            new EventStore(connectionString));
        services.AddSingleton<DatabaseManager>(_ =>
            new DatabaseManager(connectionString));
        services.AddSingleton<CalendarStore>(_ =>
            new CalendarStore(connectionString));

        // Register Services
        services.AddSingleton<ICalendarProvider, LocalCalendarProvider>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<CalendarViewModel>();
        services.AddTransient<CalendarSettingsViewModel>();
        services.AddTransient<EventEditViewModel>();

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        // Initialize database schema
        var metaStore = serviceProvider.GetRequiredService<DatabaseManager>();
        if (!metaStore.DatabaseExistsAsync(CancellationToken.None).GetAwaiter().GetResult())
        {
            Log.Information("Database not found. Creating new database at {DbPath}", dbPath);
            DatabaseManager.CreateDatabaseFileAsync(connectionString, CancellationToken.None).GetAwaiter().GetResult();
        }
        metaStore.InitializeDatabaseAsync().GetAwaiter().GetResult();

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