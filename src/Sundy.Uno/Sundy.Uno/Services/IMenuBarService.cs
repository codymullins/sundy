namespace Sundy.Uno.Services;

/// <summary>
/// Service for managing the macOS menu bar status item with mini calendar.
/// On non-macOS platforms, this service does nothing.
/// </summary>
public interface IMenuBarService
{
    /// <summary>
    /// Initializes the menu bar status item. Should be called after app launch.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Updates the displayed next event information.
    /// </summary>
    /// <param name="eventTitle">The title of the next event, or null if none.</param>
    /// <param name="eventTime">The time of the next event, or null if none.</param>
    void UpdateNextEvent(string? eventTitle, DateTime? eventTime);

    /// <summary>
    /// Updates the currently displayed month in the mini calendar.
    /// </summary>
    /// <param name="year">The year to display.</param>
    /// <param name="month">The month to display (1-12).</param>
    void UpdateDisplayedMonth(int year, int month);

    /// <summary>
    /// Cleans up the menu bar status item. Should be called when app is shutting down.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Event raised when a date is clicked in the mini calendar.
    /// </summary>
    event Action<DateTime>? DateClicked;
}
