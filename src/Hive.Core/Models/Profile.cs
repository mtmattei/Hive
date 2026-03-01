namespace Hive.Core.Models;

public class Profile
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#1B6B93";
    public string? Emoji { get; set; }
    public int StarBalance { get; set; }
    public int SortOrder { get; set; }

    public FamilyAccount? FamilyAccount { get; set; }
    public Guid? LinkedCalendarId { get; set; }
    public SyncedCalendar? LinkedCalendar { get; set; }

    public ICollection<CalendarEvent> Events { get; set; } = [];
    public ICollection<TaskAssignment> TaskAssignments { get; set; } = [];
    public ICollection<RewardEligibility> EligibleRewards { get; set; } = [];
}
