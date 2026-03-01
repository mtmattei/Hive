using Hive.Core.Models;

namespace Hive.Core.Recurrence;

public static class RecurrenceCalculator
{
    /// <summary>
    /// Expands a recurrence rule into concrete DateTimeOffset instances within the given range.
    /// </summary>
    public static IReadOnlyList<DateTimeOffset> ExpandOccurrences(
        DateTimeOffset start,
        RecurrenceRule rule,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        int maxOccurrences = 365)
    {
        var occurrences = new List<DateTimeOffset>();
        var effectiveEnd = rule.RepeatUntil.HasValue
            ? (rangeEnd < rule.RepeatUntil.Value ? rangeEnd : rule.RepeatUntil.Value)
            : rangeEnd;

        var candidate = start;
        var count = 0;

        while (candidate <= effectiveEnd && count < maxOccurrences)
        {
            if (candidate >= rangeStart && ShouldInclude(candidate, rule))
            {
                occurrences.Add(candidate);
            }

            candidate = AdvanceByFrequency(candidate, rule);
            count++;
        }

        return occurrences;
    }

    /// <summary>
    /// Generates CalendarEvent instances from a recurring event template within the range.
    /// </summary>
    public static IReadOnlyList<CalendarEvent> ExpandEvent(
        CalendarEvent template,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd)
    {
        if (template.Recurrence is null)
            return [template];

        var duration = template.EndTime.HasValue
            ? template.EndTime.Value - template.StartTime
            : TimeSpan.Zero;

        var occurrences = ExpandOccurrences(
            template.StartTime,
            template.Recurrence,
            rangeStart,
            rangeEnd);

        return occurrences.Select(startTime => new CalendarEvent
        {
            Id = template.Id,
            FamilyAccountId = template.FamilyAccountId,
            Title = template.Title,
            StartTime = startTime,
            EndTime = duration > TimeSpan.Zero ? startTime + duration : null,
            IsAllDay = template.IsAllDay,
            Notes = template.Notes,
            Location = template.Location,
            ProfileId = template.ProfileId,
            Profile = template.Profile,
            Recurrence = template.Recurrence,
            SyncedCalendarId = template.SyncedCalendarId,
            ExternalEventId = template.ExternalEventId,
            ReminderMinutesBefore = template.ReminderMinutesBefore,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
        }).ToList();
    }

    private static bool ShouldInclude(DateTimeOffset candidate, RecurrenceRule rule)
    {
        if (rule.Frequency == RecurrenceFrequency.Weekly && rule.DaysOfWeek is { Length: > 0 })
        {
            return rule.DaysOfWeek.Contains(candidate.DayOfWeek);
        }

        if (rule.Frequency == RecurrenceFrequency.Monthly && rule.DayOfMonth.HasValue)
        {
            return candidate.Day == rule.DayOfMonth.Value;
        }

        return true;
    }

    private static DateTimeOffset AdvanceByFrequency(DateTimeOffset current, RecurrenceRule rule)
    {
        return rule.Frequency switch
        {
            RecurrenceFrequency.Daily => current.AddDays(rule.Interval),
            RecurrenceFrequency.Weekly => AdvanceWeekly(current, rule),
            RecurrenceFrequency.Monthly => current.AddMonths(rule.Interval),
            RecurrenceFrequency.Yearly => current.AddYears(rule.Interval),
            _ => current.AddDays(1),
        };
    }

    private static DateTimeOffset AdvanceWeekly(DateTimeOffset current, RecurrenceRule rule)
    {
        if (rule.DaysOfWeek is not { Length: > 0 })
            return current.AddDays(7 * rule.Interval);

        // For weekly with specific days, advance one day at a time until we hit the next matching day.
        // When we wrap past the last day in the set, skip by (interval - 1) weeks.
        var sorted = rule.DaysOfWeek.OrderBy(d => d).ToArray();
        var currentIndex = Array.IndexOf(sorted, current.DayOfWeek);

        if (currentIndex >= 0 && currentIndex < sorted.Length - 1)
        {
            // Move to next day in the set within the same week
            var nextDay = sorted[currentIndex + 1];
            var daysToAdd = ((int)nextDay - (int)current.DayOfWeek + 7) % 7;
            return current.AddDays(daysToAdd);
        }

        // Wrap to first day of next interval
        var firstDay = sorted[0];
        var daysUntilFirst = ((int)firstDay - (int)current.DayOfWeek + 7) % 7;
        if (daysUntilFirst == 0) daysUntilFirst = 7;
        return current.AddDays(daysUntilFirst + 7 * (rule.Interval - 1));
    }
}
