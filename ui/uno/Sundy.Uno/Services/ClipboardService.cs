using Windows.ApplicationModel.DataTransfer;

namespace Sundy.Uno.Services;

/// <summary>
/// Uno Platform clipboard service using Windows.ApplicationModel.DataTransfer.Clipboard API.
/// Migrated from Avalonia ClipboardService - preserves same interface.
/// Doc reference: https://platform.uno/docs/articles/features/clipboard.html
/// </summary>
public class ClipboardService : IClipboardService
{
    public Task SetTextAsync(string text)
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(text);
        Clipboard.SetContent(dataPackage);
        return Task.CompletedTask;
    }
}
