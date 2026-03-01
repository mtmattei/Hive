namespace Hive.Core.Models;

/// <summary>
/// Represents a countdown to a special event displayed on the calendar.
/// </summary>
public class EventCountdown
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Emoji { get; set; }
    public DateOnly TargetDate { get; set; }
    public bool IsActive { get; set; } = true;

    public int DaysRemaining => (TargetDate.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days;
    public bool IsToday => DaysRemaining == 0;
    public bool IsPast => DaysRemaining < 0;
}
