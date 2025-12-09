using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Sundy.Avalonia.Test.AvaloniaAppInitializer))]

namespace Sundy.Avalonia.Test;

public class AvaloniaAppInitializer
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = true
        });
}
