using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Sundy;

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        // Initialize SQLite for WebAssembly
        SQLitePCL.Batteries_V2.Init();
        
        return BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}