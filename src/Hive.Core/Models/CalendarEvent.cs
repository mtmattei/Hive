namespace Hive.Core.Models;

public class CalendarEvent
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public bool IsAllDay { get; set; }
    public string? Notes { get; set; }
    public string? Location { get; set; }

    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }

    public RecurrenceRule? Recurrence { get; set; }

    public Guid? SyncedCalendarId { get; set; }
    public string? ExternalEventId { get; set; }

    public int? ReminderMinutesBefore { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
