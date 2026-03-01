namespace Hive.Core.Models;

public class RecurrenceRule
{
    public Guid Id { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public int Interval { get; set; } = 1;
    public DayOfWeek[]? DaysOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public DateTimeOffset? RepeatUntil { get; set; }
}

public enum RecurrenceFrequency
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}
