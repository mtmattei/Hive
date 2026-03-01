using Hive.Core.Models;

namespace Hive.Core.Services;

public record SyncResult(
    int EventsPulled,
    int EventsPushed,
    int Conflicts,
    IReadOnlyList<string> Errors);

public record ExternalCalendarInfo(
    string ExternalId,
    string Name,
    string? Color);

public interface ISyncService
{
    CalendarProvider Provider { get; }

    Task<SyncResult> PullEventsAsync(
        SyncedCalendar calendar,
        CancellationToken ct = default);

    Task<SyncResult> PushEventAsync(
        SyncedCalendar calendar,
        CalendarEvent evt,
        CancellationToken ct = default);

    Task<IReadOnlyList<ExternalCalendarInfo>> ListAvailableCalendarsAsync(
        string accessToken,
        CancellationToken ct = default);
}
