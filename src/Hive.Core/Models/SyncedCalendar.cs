namespace Hive.Core.Models;

public class SyncedCalendar
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public CalendarProvider Provider { get; set; }
    public SyncDirection Direction { get; set; }
    public string AccountEmail { get; set; } = string.Empty;
    public string? ExternalCalendarId { get; set; }
    public string? ExternalCalendarName { get; set; }
    public string? Color { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? LastSyncedAt { get; set; }

    public Guid? LinkedProfileId { get; set; }
    public Profile? LinkedProfile { get; set; }
}

public enum CalendarProvider
{
    Google,
    ICloud,
    Outlook,
    Cozi,
    Yahoo,
    IcsUrl
}

public enum SyncDirection
{
    OneWay,
    TwoWay
}
