namespace Hive.Core.Models;

public class TaskItem
{
    public Guid Id { get; set; }
    public Guid FamilyAccountId { get; set; }
    public TaskItemType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Emoji { get; set; }
    public int StarValue { get; set; }

    public ICollection<TaskAssignment> Assignments { get; set; } = [];

    // Chore scheduling
    public DateTimeOffset? DueDate { get; set; }
    public TimeOnly? DueTime { get; set; }
    public RecurrenceRule? Recurrence { get; set; }

    // Routine scheduling
    public RoutineTimeOfDay? RoutineTimeOfDay { get; set; }
    public DayOfWeek[]? RoutineDays { get; set; }

    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class TaskAssignment
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }
}

public class TaskCompletion
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }
    public DateOnly CompletedDate { get; set; }
    public RoutineTimeOfDay? CompletedTimeOfDay { get; set; }
    public int StarsEarned { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
}

public enum TaskItemType
{
    Chore,
    Routine
}

public enum RoutineTimeOfDay
{
    Morning,
    Afternoon,
    Evening
}
