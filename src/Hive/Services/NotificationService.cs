using Hive.Core.Models;
using Hive.Core.Services;
using Microsoft.Extensions.Logging;

namespace Hive.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly HashSet<string> _scheduledIds = [];

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task<bool> RequestPermissionAsync(CancellationToken ct = default)
    {
        // On WinUI/Uno, notification permission is granted by default.
        // On mobile platforms, this would use platform-specific APIs.
        _logger.LogInformation("Notification permission requested");
        return Task.FromResult(true);
    }

    public Task ScheduleEventReminderAsync(CalendarEvent evt, CancellationToken ct = default)
    {
        if (!evt.ReminderMinutesBefore.HasValue || evt.ReminderMinutesBefore.Value <= 0)
            return Task.CompletedTask;

        var reminderTime = evt.StartTime.AddMinutes(-evt.ReminderMinutesBefore.Value);
        if (reminderTime <= DateTimeOffset.Now)
            return Task.CompletedTask;

        var notificationId = $"event-{evt.Id}";
        _scheduledIds.Add(notificationId);

        _logger.LogInformation(
            "Scheduled reminder for event '{Title}' at {Time}",
            evt.Title, reminderTime);

        // Platform-specific notification scheduling would go here.
        // Windows: ToastNotificationManager
        // Android: AlarmManager / WorkManager
        // iOS: UNUserNotificationCenter
        SchedulePlatformNotification(
            notificationId,
            $"Upcoming: {evt.Title}",
            GetReminderBody(evt),
            reminderTime);

        return Task.CompletedTask;
    }

    public Task CancelEventReminderAsync(Guid eventId, CancellationToken ct = default)
    {
        var notificationId = $"event-{eventId}";
        _scheduledIds.Remove(notificationId);

        _logger.LogInformation("Cancelled reminder for event {EventId}", eventId);

        CancelPlatformNotification(notificationId);
        return Task.CompletedTask;
    }

    public Task ScheduleTaskReminderAsync(TaskItem task, DateOnly date, CancellationToken ct = default)
    {
        var notificationId = $"task-{task.Id}-{date:yyyyMMdd}";
        _scheduledIds.Add(notificationId);

        var reminderTime = task.RoutineTimeOfDay switch
        {
            RoutineTimeOfDay.Morning => new DateTimeOffset(date.Year, date.Month, date.Day, 7, 0, 0, TimeSpan.Zero),
            RoutineTimeOfDay.Afternoon => new DateTimeOffset(date.Year, date.Month, date.Day, 13, 0, 0, TimeSpan.Zero),
            RoutineTimeOfDay.Evening => new DateTimeOffset(date.Year, date.Month, date.Day, 18, 0, 0, TimeSpan.Zero),
            _ => new DateTimeOffset(date.Year, date.Month, date.Day, 8, 0, 0, TimeSpan.Zero),
        };

        if (reminderTime <= DateTimeOffset.Now)
            return Task.CompletedTask;

        _logger.LogInformation(
            "Scheduled reminder for task '{Title}' at {Time}",
            task.Title, reminderTime);

        SchedulePlatformNotification(
            notificationId,
            $"Task reminder: {task.Title}",
            $"{task.Emoji ?? "\U0001f4cb"} Time to {task.Title.ToLowerInvariant()}!",
            reminderTime);

        return Task.CompletedTask;
    }

    public Task CancelAllAsync(CancellationToken ct = default)
    {
        foreach (var id in _scheduledIds)
            CancelPlatformNotification(id);

        _scheduledIds.Clear();
        _logger.LogInformation("Cancelled all scheduled notifications");
        return Task.CompletedTask;
    }

    private static string GetReminderBody(CalendarEvent evt)
    {
        var parts = new List<string>();
        if (evt.IsAllDay)
            parts.Add("All day event");
        else
            parts.Add($"Starts at {evt.StartTime:h:mm tt}");

        if (!string.IsNullOrEmpty(evt.Location))
            parts.Add($"at {evt.Location}");

        return string.Join(" ", parts);
    }

    private void SchedulePlatformNotification(
        string id, string title, string body, DateTimeOffset scheduledTime)
    {
#if WINDOWS
        try
        {
            var content = new Windows.UI.Notifications.ToastContentBuilder()
                .AddText(title)
                .AddText(body)
                .GetToastContent();

            var notification = new Windows.UI.Notifications.ScheduledToastNotification(content.GetXml(), scheduledTime)
            {
                Id = id,
                Tag = id,
            };

            Windows.UI.Notifications.ToastNotificationManager
                .CreateToastNotifier()
                .AddToSchedule(notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to schedule Windows toast notification");
        }
#else
        _logger.LogDebug(
            "Platform notification stub: {Id} '{Title}' at {Time}",
            id, title, scheduledTime);
#endif
    }

    private void CancelPlatformNotification(string id)
    {
#if WINDOWS
        try
        {
            var notifier = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            var scheduled = notifier.GetScheduledToastNotifications();
            var match = scheduled.FirstOrDefault(n => n.Id == id);
            if (match is not null)
                notifier.RemoveFromSchedule(match);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cancel Windows toast notification");
        }
#else
        _logger.LogDebug("Platform notification cancel stub: {Id}", id);
#endif
    }
}
