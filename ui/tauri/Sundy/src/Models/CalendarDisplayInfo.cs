namespace Sundy.Models;

public class CalendarDisplayInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    /// <summary>
    /// Custom display name override. When set, this is shown instead of Name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// For external calendars, the original name from the provider (for tooltip).
    /// Null for local calendars.
    /// </summary>
    public string? OriginalName { get; set; }

    /// <summary>
    /// The name to display in the UI. Uses DisplayName if set, otherwise Name.
    /// </summary>
    public string EffectiveName => DisplayName ?? Name;

    public string Color { get; set; } = "#4285f4";
    public bool IsVisible { get; set; } = true;
    public bool IsHidden { get; set; }
    public string? AccountId { get; set; }
    public string? AccountEmail { get; set; }
}
