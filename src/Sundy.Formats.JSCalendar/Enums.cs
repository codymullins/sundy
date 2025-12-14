using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Sundy.Formats.JSCalendar;

/// <summary>
/// The frequency of recurrence.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<RecurrenceFrequency>))]
public enum RecurrenceFrequency
{
    [EnumMember(Value = "yearly")]
    Yearly,

    [EnumMember(Value = "monthly")]
    Monthly,

    [EnumMember(Value = "weekly")]
    Weekly,

    [EnumMember(Value = "daily")]
    Daily,

    [EnumMember(Value = "hourly")]
    Hourly,

    [EnumMember(Value = "minutely")]
    Minutely,

    [EnumMember(Value = "secondly")]
    Secondly
}

/// <summary>
/// Behavior to use when expansion of recurrence produces invalid dates.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SkipBehavior>))]
public enum SkipBehavior
{
    [EnumMember(Value = "omit")]
    Omit,

    [EnumMember(Value = "backward")]
    Backward,

    [EnumMember(Value = "forward")]
    Forward
}

/// <summary>
/// Day of the week.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DayOfWeek>))]
public enum DayOfWeek
{
    [EnumMember(Value = "mo")]
    Monday,

    [EnumMember(Value = "tu")]
    Tuesday,

    [EnumMember(Value = "we")]
    Wednesday,

    [EnumMember(Value = "th")]
    Thursday,

    [EnumMember(Value = "fr")]
    Friday,

    [EnumMember(Value = "sa")]
    Saturday,

    [EnumMember(Value = "su")]
    Sunday
}

/// <summary>
/// The scheduling status of an event.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<EventStatus>))]
public enum EventStatus
{
    [EnumMember(Value = "confirmed")]
    Confirmed,

    [EnumMember(Value = "cancelled")]
    Cancelled,

    [EnumMember(Value = "tentative")]
    Tentative
}

/// <summary>
/// Progress status for tasks and participants.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProgressStatus>))]
public enum ProgressStatus
{
    [EnumMember(Value = "needs-action")]
    NeedsAction,

    [EnumMember(Value = "in-process")]
    InProcess,

    [EnumMember(Value = "completed")]
    Completed,

    [EnumMember(Value = "failed")]
    Failed,

    [EnumMember(Value = "cancelled")]
    Cancelled
}

/// <summary>
/// Free/busy status.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<FreeBusyStatus>))]
public enum FreeBusyStatus
{
    [EnumMember(Value = "free")]
    Free,

    [EnumMember(Value = "busy")]
    Busy
}

/// <summary>
/// Privacy classification.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Privacy>))]
public enum Privacy
{
    [EnumMember(Value = "public")]
    Public,

    [EnumMember(Value = "private")]
    Private,

    [EnumMember(Value = "secret")]
    Secret
}

/// <summary>
/// Participant kind.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ParticipantKind>))]
public enum ParticipantKind
{
    [EnumMember(Value = "individual")]
    Individual,

    [EnumMember(Value = "group")]
    Group,

    [EnumMember(Value = "location")]
    Location,

    [EnumMember(Value = "resource")]
    Resource
}

/// <summary>
/// Participant roles.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ParticipantRole>))]
public enum ParticipantRole
{
    [EnumMember(Value = "owner")]
    Owner,

    [EnumMember(Value = "optional")]
    Optional,

    [EnumMember(Value = "informational")]
    Informational,

    [EnumMember(Value = "chair")]
    Chair,

    [EnumMember(Value = "required")]
    Required
}

/// <summary>
/// Participation status.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ParticipationStatus>))]
public enum ParticipationStatus
{
    [EnumMember(Value = "needs-action")]
    NeedsAction,

    [EnumMember(Value = "accepted")]
    Accepted,

    [EnumMember(Value = "declined")]
    Declined,

    [EnumMember(Value = "tentative")]
    Tentative,

    [EnumMember(Value = "delegated")]
    Delegated
}

/// <summary>
/// Alert action type.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AlertAction>))]
public enum AlertAction
{
    [EnumMember(Value = "display")]
    Display,

    [EnumMember(Value = "email")]
    Email
}

/// <summary>
/// Trigger relative-to property.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<RelativeTo>))]
public enum RelativeTo
{
    [EnumMember(Value = "start")]
    Start,

    [EnumMember(Value = "end")]
    End
}

/// <summary>
/// Link display purposes.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<LinkDisplay>))]
public enum LinkDisplay
{
    [EnumMember(Value = "badge")]
    Badge,

    [EnumMember(Value = "graphic")]
    Graphic,

    [EnumMember(Value = "fullsize")]
    Fullsize,

    [EnumMember(Value = "thumbnail")]
    Thumbnail
}

/// <summary>
/// Virtual location features.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<VirtualLocationFeature>))]
public enum VirtualLocationFeature
{
    [EnumMember(Value = "audio")]
    Audio,

    [EnumMember(Value = "chat")]
    Chat,

    [EnumMember(Value = "feed")]
    Feed,

    [EnumMember(Value = "moderator")]
    Moderator,

    [EnumMember(Value = "phone")]
    Phone,

    [EnumMember(Value = "screen")]
    Screen,

    [EnumMember(Value = "video")]
    Video
}

/// <summary>
/// Relation types.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<RelationType>))]
public enum RelationType
{
    [EnumMember(Value = "first")]
    First,

    [EnumMember(Value = "next")]
    Next,

    [EnumMember(Value = "child")]
    Child,

    [EnumMember(Value = "parent")]
    Parent,

    [EnumMember(Value = "snooze")]
    Snooze
}
