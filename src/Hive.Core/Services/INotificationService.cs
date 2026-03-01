using Hive.Core.Models;

namespace Hive.Core.Services;

public interface INotificationService
{
    Task ScheduleEventReminderAsync(CalendarEvent evt, CancellationToken ct = default);
    Task CancelEventReminderAsync(Guid eventId, CancellationToken ct = default);
    Task ScheduleTaskReminderAsync(TaskItem task, DateOnly date, CancellationToken ct = default);
    Task CancelAllAsync(CancellationToken ct = default);
    Task<bool> RequestPermissionAsync(CancellationToken ct = default);
}
