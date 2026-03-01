using Hive.Core.Models;

namespace Hive.Core.Services;

public interface ICalendarService
{
    Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        Guid familyAccountId,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        IEnumerable<Guid>? profileFilter = null,
        CancellationToken ct = default);

    Task<CalendarEvent?> GetEventByIdAsync(Guid eventId, CancellationToken ct = default);

    Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent, CancellationToken ct = default);

    Task<CalendarEvent> UpdateEventAsync(CalendarEvent calendarEvent, CancellationToken ct = default);

    Task DeleteEventAsync(Guid eventId, CancellationToken ct = default);

    Task<IReadOnlyList<CalendarEvent>> ExpandRecurringEventsAsync(
        Guid familyAccountId,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        CancellationToken ct = default);
}
