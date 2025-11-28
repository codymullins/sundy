using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        
        // Get the application data path
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var sundyDataPath = System.IO.Path.Combine(appDataPath, "Sundy");
        System.IO.Directory.CreateDirectory(sundyDataPath);
        var dbPath = System.IO.Path.Combine(sundyDataPath, "sundy.db");
        
        // Register DbContext with SQLite
        services.AddDbContext<SundyDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));
        
        // Register Services
        services.AddSingleton<ICalendarProvider, LocalCalendarProvider>();
        services.AddSingleton<BlockingEngine>();
        
        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<CalendarViewModel>();
        services.AddTransient<CalendarSettingsViewModel>();
        services.AddTransient<EventEditViewModel>();
        
        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();
        
        // Ensure database is created
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SundyDbContext>();
            db.Database.EnsureCreated();
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
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // Get MainViewModel from DI
            var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
            
            singleViewPlatform.MainView = new MainView
            {
                DataContext = mainViewModel
            };
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