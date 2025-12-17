using Avalonia;
using Avalonia.Logging;

namespace Sundy.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var sundyDataPath = Path.Combine(appDataPath, "sundy");
        var logsPath = Path.Combine(sundyDataPath, "logs");
        Directory.CreateDirectory(logsPath);
        var logPath = Path.Combine(logsPath, "sundy-.log");

        try
        {
            // Log.Information("Starting Sundy application");
            BuildAvaloniaApp().WithDeveloperTools().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            // Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            // Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace(LogEventLevel.Information);
}
