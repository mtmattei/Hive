namespace Hive.Core.Models;

public class CalendarSettings
{
    public Guid FamilyAccountId { get; set; }
    public FamilyAccount? FamilyAccount { get; set; }
    public DayOfWeek StartWeekOn { get; set; } = DayOfWeek.Sunday;
    public int ScheduleViewDays { get; set; } = 5;
    public bool StartOnCurrentDay { get; set; } = true;
    public bool DimPastEvents { get; set; } = true;
    public bool ShadeWeekends { get; set; }
    public bool PreviewChoresInCalendar { get; set; }
    public string? CalendarDisplayName { get; set; }
    public bool ParentalLockEnabled { get; set; }
    public string? ParentalLockPinHash { get; set; }
    public TimeOnly? SleepFrom { get; set; }
    public TimeOnly? SleepTo { get; set; }
    public bool SleepScheduleEnabled { get; set; }
}
