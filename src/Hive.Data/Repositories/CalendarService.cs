using Hive.Core.Models;
using Hive.Core.Recurrence;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

public class CalendarService : ICalendarService
{
    private readonly HiveDbContext _db;

    public CalendarService(HiveDbContext db) => _db = db;

    public async Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(
        Guid familyAccountId,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        IEnumerable<Guid>? profileFilter = null,
        CancellationToken ct = default)
    {
        var query = _db.CalendarEvents
            .Include(e => e.Profile)
            .Include(e => e.Recurrence)
            .Where(e => e.FamilyAccountId == familyAccountId);

        if (profileFilter is not null)
        {
            var profileIds = profileFilter.ToHashSet();
            query = query.Where(e => profileIds.Contains(e.ProfileId));
        }

        // Get non-recurring events in range
        var nonRecurring = await query
            .Where(e => e.Recurrence == null)
            .Where(e => e.StartTime <= rangeEnd && (e.EndTime ?? e.StartTime) >= rangeStart)
            .ToListAsync(ct);

        // Get recurring event templates and expand them
        var recurring = await query
            .Where(e => e.Recurrence != null)
            .ToListAsync(ct);

        var expanded = recurring.SelectMany(e => RecurrenceCalculator.ExpandEvent(e, rangeStart, rangeEnd));

        return nonRecurring.Concat(expanded)
            .OrderBy(e => e.StartTime)
            .ToList();
    }

    public async Task<CalendarEvent?> GetEventByIdAsync(Guid eventId, CancellationToken ct = default)
    {
        return await _db.CalendarEvents
            .Include(e => e.Profile)
            .Include(e => e.Recurrence)
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);
    }

    public async Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent, CancellationToken ct = default)
    {
        calendarEvent.Id = calendarEvent.Id == Guid.Empty ? Guid.NewGuid() : calendarEvent.Id;
        calendarEvent.CreatedAt = DateTimeOffset.UtcNow;
        calendarEvent.UpdatedAt = DateTimeOffset.UtcNow;

        _db.CalendarEvents.Add(calendarEvent);
        await _db.SaveChangesAsync(ct);
        return calendarEvent;
    }

    public async Task<CalendarEvent> UpdateEventAsync(CalendarEvent calendarEvent, CancellationToken ct = default)
    {
        calendarEvent.UpdatedAt = DateTimeOffset.UtcNow;
        _db.CalendarEvents.Update(calendarEvent);
        await _db.SaveChangesAsync(ct);
        return calendarEvent;
    }

    public async Task DeleteEventAsync(Guid eventId, CancellationToken ct = default)
    {
        var evt = await _db.CalendarEvents.FindAsync([eventId], ct);
        if (evt is not null)
        {
            _db.CalendarEvents.Remove(evt);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<CalendarEvent>> ExpandRecurringEventsAsync(
        Guid familyAccountId,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        CancellationToken ct = default)
    {
        var recurring = await _db.CalendarEvents
            .Include(e => e.Profile)
            .Include(e => e.Recurrence)
            .Where(e => e.FamilyAccountId == familyAccountId && e.Recurrence != null)
            .ToListAsync(ct);

        return recurring
            .SelectMany(e => RecurrenceCalculator.ExpandEvent(e, rangeStart, rangeEnd))
            .OrderBy(e => e.StartTime)
            .ToList();
    }
}
