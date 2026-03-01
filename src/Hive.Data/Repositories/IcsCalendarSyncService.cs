using System.Net.Http;
using System.Text.RegularExpressions;
using Hive.Core.Models;
using Hive.Core.Services;
using Microsoft.Extensions.Logging;

namespace Hive.Data.Repositories;

public partial class IcsCalendarSyncService : ISyncService
{
    private readonly ICalendarService _calendarService;
    private readonly ILogger<IcsCalendarSyncService> _logger;
    private static readonly HttpClient HttpClient = new();

    public CalendarProvider Provider => CalendarProvider.IcsUrl;

    public IcsCalendarSyncService(ICalendarService calendarService, ILogger<IcsCalendarSyncService> logger)
    {
        _calendarService = calendarService;
        _logger = logger;
    }

    public async Task<SyncResult> PullEventsAsync(SyncedCalendar calendar, CancellationToken ct = default)
    {
        var errors = new List<string>();
        var eventsPulled = 0;

        try
        {
            if (string.IsNullOrEmpty(calendar.ExternalCalendarId))
            {
                errors.Add("No ICS URL configured");
                return new SyncResult(0, 0, 0, errors);
            }

            var icsContent = await HttpClient.GetStringAsync(calendar.ExternalCalendarId, ct);
            var parsed = ParseIcsContent(icsContent, calendar);

            foreach (var evt in parsed)
            {
                evt.FamilyAccountId = calendar.FamilyAccountId;
                if (calendar.LinkedProfileId.HasValue)
                    evt.ProfileId = calendar.LinkedProfileId.Value;

                await _calendarService.CreateEventAsync(evt, ct);
                eventsPulled++;
            }

            _logger.LogInformation(
                "Pulled {Count} events from ICS feed {Url}",
                eventsPulled, calendar.ExternalCalendarId);
        }
        catch (HttpRequestException ex)
        {
            errors.Add($"Failed to fetch ICS feed: {ex.Message}");
            _logger.LogWarning(ex, "Failed to fetch ICS feed");
        }
        catch (Exception ex)
        {
            errors.Add($"Sync error: {ex.Message}");
            _logger.LogError(ex, "ICS sync error");
        }

        return new SyncResult(eventsPulled, 0, 0, errors);
    }

    public Task<SyncResult> PushEventAsync(SyncedCalendar calendar, CalendarEvent evt, CancellationToken ct = default)
    {
        // ICS URL feeds are read-only
        return Task.FromResult(new SyncResult(0, 0, 0, new[] { "ICS feeds are read-only" }));
    }

    public Task<IReadOnlyList<ExternalCalendarInfo>> ListAvailableCalendarsAsync(
        string accessToken, CancellationToken ct = default)
    {
        // For ICS URL, the "accessToken" is the URL itself
        var result = new List<ExternalCalendarInfo>
        {
            new(accessToken, "ICS Feed", null),
        };
        return Task.FromResult<IReadOnlyList<ExternalCalendarInfo>>(result);
    }

    private static List<CalendarEvent> ParseIcsContent(string ics, SyncedCalendar calendar)
    {
        var events = new List<CalendarEvent>();
        var veventBlocks = VEventRegex().Matches(ics);

        foreach (Match block in veventBlocks)
        {
            var content = block.Groups[1].Value;

            var title = GetIcsProperty(content, "SUMMARY");
            var dtStart = GetIcsProperty(content, "DTSTART");
            var dtEnd = GetIcsProperty(content, "DTEND");
            var location = GetIcsProperty(content, "LOCATION");
            var description = GetIcsProperty(content, "DESCRIPTION");
            var uid = GetIcsProperty(content, "UID");

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(dtStart))
                continue;

            var startTime = ParseIcsDateTime(dtStart);
            var endTime = !string.IsNullOrEmpty(dtEnd) ? ParseIcsDateTime(dtEnd) : (DateTimeOffset?)null;
            var isAllDay = dtStart.Length == 8; // YYYYMMDD format = all day

            events.Add(new CalendarEvent
            {
                Id = Guid.NewGuid(),
                FamilyAccountId = calendar.FamilyAccountId,
                ProfileId = calendar.LinkedProfileId ?? Guid.Empty,
                Title = UnescapeIcs(title),
                StartTime = startTime,
                EndTime = endTime,
                IsAllDay = isAllDay,
                Location = string.IsNullOrEmpty(location) ? null : UnescapeIcs(location),
                Notes = string.IsNullOrEmpty(description) ? null : UnescapeIcs(description),
                ExternalEventId = uid,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
        }

        return events;
    }

    private static string? GetIcsProperty(string content, string property)
    {
        var pattern = $@"(?:^|\n){property}(?:;[^:]*)?:(.+?)(?:\r?\n(?![ \t])|$)";
        var match = Regex.Match(content, pattern, RegexOptions.Singleline);
        if (!match.Success) return null;

        // Handle folded lines (continuation lines start with space or tab)
        var value = match.Groups[1].Value.Trim();
        return Regex.Replace(value, @"\r?\n[ \t]", "");
    }

    private static DateTimeOffset ParseIcsDateTime(string value)
    {
        // Strip property parameters (e.g., TZID=...)
        var clean = value.Trim();

        // YYYYMMDD (all-day)
        if (clean.Length == 8 && int.TryParse(clean, out _))
        {
            return new DateTimeOffset(
                int.Parse(clean[..4]),
                int.Parse(clean[4..6]),
                int.Parse(clean[6..8]),
                0, 0, 0, TimeSpan.Zero);
        }

        // YYYYMMDDTHHmmssZ (UTC)
        if (clean.EndsWith('Z') && clean.Length >= 15)
        {
            clean = clean.TrimEnd('Z');
            return new DateTimeOffset(
                int.Parse(clean[..4]),
                int.Parse(clean[4..6]),
                int.Parse(clean[6..8]),
                int.Parse(clean[9..11]),
                int.Parse(clean[11..13]),
                int.Parse(clean[13..15]),
                TimeSpan.Zero);
        }

        // YYYYMMDDTHHmmss (local time)
        if (clean.Length >= 15 && clean[8] == 'T')
        {
            return new DateTimeOffset(
                int.Parse(clean[..4]),
                int.Parse(clean[4..6]),
                int.Parse(clean[6..8]),
                int.Parse(clean[9..11]),
                int.Parse(clean[11..13]),
                int.Parse(clean[13..15]),
                TimeZoneInfo.Local.BaseUtcOffset);
        }

        // Fallback
        if (DateTimeOffset.TryParse(clean, out var dto))
            return dto;

        return DateTimeOffset.UtcNow;
    }

    private static string UnescapeIcs(string value) =>
        value.Replace("\\n", "\n")
             .Replace("\\,", ",")
             .Replace("\\;", ";")
             .Replace("\\\\", "\\");

    [GeneratedRegex(@"BEGIN:VEVENT\r?\n(.*?)END:VEVENT", RegexOptions.Singleline)]
    private static partial Regex VEventRegex();
}
