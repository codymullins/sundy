using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Sundy.Formats.iCalendar;

/// <summary>
/// Classification of calendar component (CLASS property).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Classification>))]
public enum Classification
{
    [EnumMember(Value = "PUBLIC")]
    Public,

    [EnumMember(Value = "PRIVATE")]
    Private,

    [EnumMember(Value = "CONFIDENTIAL")]
    Confidential
}

/// <summary>
/// Status of an event (STATUS property for VEVENT).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<EventStatus>))]
public enum EventStatus
{
    [EnumMember(Value = "TENTATIVE")]
    Tentative,

    [EnumMember(Value = "CONFIRMED")]
    Confirmed,

    [EnumMember(Value = "CANCELLED")]
    Cancelled
}

/// <summary>
/// Status of a to-do (STATUS property for VTODO).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TodoStatus>))]
public enum TodoStatus
{
    [EnumMember(Value = "NEEDS-ACTION")]
    NeedsAction,

    [EnumMember(Value = "COMPLETED")]
    Completed,

    [EnumMember(Value = "IN-PROCESS")]
    InProcess,

    [EnumMember(Value = "CANCELLED")]
    Cancelled
}

/// <summary>
/// Status of a journal entry (STATUS property for VJOURNAL).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<JournalStatus>))]
public enum JournalStatus
{
    [EnumMember(Value = "DRAFT")]
    Draft,

    [EnumMember(Value = "FINAL")]
    Final,

    [EnumMember(Value = "CANCELLED")]
    Cancelled
}

/// <summary>
/// Time transparency (TRANSP property).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TimeTransparency>))]
public enum TimeTransparency
{
    [EnumMember(Value = "OPAQUE")]
    Opaque,

    [EnumMember(Value = "TRANSPARENT")]
    Transparent
}

/// <summary>
/// Recurrence frequency (FREQ in RRULE).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<RecurrenceFrequency>))]
public enum RecurrenceFrequency
{
    [EnumMember(Value = "SECONDLY")]
    Secondly,

    [EnumMember(Value = "MINUTELY")]
    Minutely,

    [EnumMember(Value = "HOURLY")]
    Hourly,

    [EnumMember(Value = "DAILY")]
    Daily,

    [EnumMember(Value = "WEEKLY")]
    Weekly,

    [EnumMember(Value = "MONTHLY")]
    Monthly,

    [EnumMember(Value = "YEARLY")]
    Yearly
}

/// <summary>
/// Day of week (used in BYDAY recurrence rule).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Weekday>))]
public enum Weekday
{
    [EnumMember(Value = "SU")]
    Sunday,

    [EnumMember(Value = "MO")]
    Monday,

    [EnumMember(Value = "TU")]
    Tuesday,

    [EnumMember(Value = "WE")]
    Wednesday,

    [EnumMember(Value = "TH")]
    Thursday,

    [EnumMember(Value = "FR")]
    Friday,

    [EnumMember(Value = "SA")]
    Saturday
}

/// <summary>
/// Alarm action (ACTION property for VALARM).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AlarmAction>))]
public enum AlarmAction
{
    [EnumMember(Value = "AUDIO")]
    Audio,

    [EnumMember(Value = "DISPLAY")]
    Display,

    [EnumMember(Value = "EMAIL")]
    Email
}

/// <summary>
/// Participant status (PARTSTAT parameter).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ParticipantStatus>))]
public enum ParticipantStatus
{
    [EnumMember(Value = "NEEDS-ACTION")]
    NeedsAction,

    [EnumMember(Value = "ACCEPTED")]
    Accepted,

    [EnumMember(Value = "DECLINED")]
    Declined,

    [EnumMember(Value = "TENTATIVE")]
    Tentative,

    [EnumMember(Value = "DELEGATED")]
    Delegated,

    [EnumMember(Value = "COMPLETED")]
    Completed,

    [EnumMember(Value = "IN-PROCESS")]
    InProcess
}

/// <summary>
/// Participant role (ROLE parameter).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ParticipantRole>))]
public enum ParticipantRole
{
    [EnumMember(Value = "CHAIR")]
    Chair,

    [EnumMember(Value = "REQ-PARTICIPANT")]
    RequiredParticipant,

    [EnumMember(Value = "OPT-PARTICIPANT")]
    OptionalParticipant,

    [EnumMember(Value = "NON-PARTICIPANT")]
    NonParticipant
}

/// <summary>
/// Calendar user type (CUTYPE parameter).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<CalendarUserType>))]
public enum CalendarUserType
{
    [EnumMember(Value = "INDIVIDUAL")]
    Individual,

    [EnumMember(Value = "GROUP")]
    Group,

    [EnumMember(Value = "RESOURCE")]
    Resource,

    [EnumMember(Value = "ROOM")]
    Room,

    [EnumMember(Value = "UNKNOWN")]
    Unknown
}

/// <summary>
/// Free/busy time type (FBTYPE parameter).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<FreeBusyTimeType>))]
public enum FreeBusyTimeType
{
    [EnumMember(Value = "FREE")]
    Free,

    [EnumMember(Value = "BUSY")]
    Busy,

    [EnumMember(Value = "BUSY-UNAVAILABLE")]
    BusyUnavailable,

    [EnumMember(Value = "BUSY-TENTATIVE")]
    BusyTentative
}

/// <summary>
/// Relationship type (RELTYPE parameter).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<RelationshipType>))]
public enum RelationshipType
{
    [EnumMember(Value = "PARENT")]
    Parent,

    [EnumMember(Value = "CHILD")]
    Child,

    [EnumMember(Value = "SIBLING")]
    Sibling
}

/// <summary>
/// Range parameter for recurrence identifier.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Range>))]
public enum Range
{
    [EnumMember(Value = "THISANDFUTURE")]
    ThisAndFuture
}
