using Microsoft.UI.Xaml;

namespace Sundy.Uno.Services;

/// <summary>
/// Provides access to the XamlRoot for displaying ContentDialogs.
/// Required for showing dialogs from ViewModels in Uno Platform.
/// </summary>
public interface IXamlRootProvider
{
    XamlRoot? XamlRoot { get; set; }
}

/// <summary>
/// Default implementation of IXamlRootProvider.
/// </summary>
public class XamlRootProvider : IXamlRootProvider
{
    public XamlRoot? XamlRoot { get; set; }
}
