using System.Globalization;
using System.Text.RegularExpressions;
using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

/// <summary>
/// Parses pasted text, forwarded emails, or OCR output to extract event dates
/// and create calendar events automatically (Magic Import).
/// </summary>
public partial class MagicImportService : IMagicImportService
{
    private readonly HiveDbContext _db;

    public MagicImportService(HiveDbContext db) => _db = db;

    public async Task<MagicImport> ImportFromTextAsync(
        Guid familyAccountId, string content, Guid defaultProfileId, CancellationToken ct = default)
    {
        var import = new MagicImport
        {
            Id = Guid.NewGuid(),
            FamilyAccountId = familyAccountId,
            SourceType = ImportSourceType.Email,
            SourceContent = content,
            Status = ImportStatus.Processing,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        try
        {
            var events = ParseEvents(content, familyAccountId, defaultProfileId);
            foreach (var evt in events)
            {
                _db.CalendarEvents.Add(evt);
            }

            import.EventsCreated = events.Count;
            import.Status = ImportStatus.Completed;
        }
        catch (Exception ex)
        {
            import.Status = ImportStatus.Failed;
            import.ErrorMessage = ex.Message;
        }

        _db.MagicImports.Add(import);
        await _db.SaveChangesAsync(ct);
        return import;
    }

    public async Task<IReadOnlyList<MagicImport>> GetImportHistoryAsync(
        Guid familyAccountId, CancellationToken ct = default)
    {
        return await _db.MagicImports
            .Where(i => i.FamilyAccountId == familyAccountId)
            .OrderByDescending(i => i.CreatedAt)
            .Take(20)
            .ToListAsync(ct);
    }

    private static List<CalendarEvent> ParseEvents(
        string content, Guid familyAccountId, Guid profileId)
    {
        var events = new List<CalendarEvent>();
        var now = DateTimeOffset.UtcNow;

        // Try to find date patterns and associated text
        var lines = content.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            // Match patterns like "March 15, 2026 - Soccer Tournament" or "3/15/2026 Parent Night 6pm"
            var dateMatch = DatePatternRegex().Match(line);
            if (!dateMatch.Success) continue;

            if (!TryParseFlexibleDate(dateMatch.Value, out var parsedDate)) continue;

            // Extract the event title — text after the date
            var title = line[dateMatch.Index..].Replace(dateMatch.Value, "").Trim(' ', '-', ':', '|', '\t');
            if (string.IsNullOrWhiteSpace(title))
                title = $"Event on {parsedDate:MMM d}";

            // Try to extract time
            var timeMatch = TimePatternRegex().Match(line);
            var startTime = new DateTimeOffset(parsedDate, TimeSpan.Zero);
            var isAllDay = true;

            if (timeMatch.Success && TryParseTime(timeMatch.Value, out var time))
            {
                startTime = new DateTimeOffset(parsedDate.Add(time), TimeSpan.Zero);
                isAllDay = false;
            }

            events.Add(new CalendarEvent
            {
                Id = Guid.NewGuid(),
                FamilyAccountId = familyAccountId,
                ProfileId = profileId,
                Title = title.Length > 100 ? title[..100] : title,
                StartTime = startTime,
                EndTime = isAllDay ? null : startTime.AddHours(1),
                IsAllDay = isAllDay,
                Notes = "Imported via Magic Import",
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        return events;
    }

    private static bool TryParseFlexibleDate(string text, out DateTime date)
    {
        var formats = new[]
        {
            "M/d/yyyy", "MM/dd/yyyy", "M-d-yyyy",
            "MMMM d, yyyy", "MMMM d yyyy", "MMM d, yyyy", "MMM d yyyy",
            "yyyy-MM-dd",
        };

        return DateTime.TryParseExact(text.Trim(), formats,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    private static bool TryParseTime(string text, out TimeSpan time)
    {
        text = text.Trim().ToLowerInvariant();
        var match = Regex.Match(text, @"(\d{1,2})(?::(\d{2}))?\s*(am|pm)?");
        if (!match.Success) { time = default; return false; }

        var hour = int.Parse(match.Groups[1].Value);
        var min = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        var ampm = match.Groups[3].Value;

        if (ampm == "pm" && hour < 12) hour += 12;
        if (ampm == "am" && hour == 12) hour = 0;

        time = new TimeSpan(hour, min, 0);
        return true;
    }

    [GeneratedRegex(@"\b(?:\d{1,2}[/-]\d{1,2}[/-]\d{2,4}|(?:January|February|March|April|May|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s+\d{1,2},?\s*\d{4}|\d{4}-\d{2}-\d{2})\b", RegexOptions.IgnoreCase)]
    private static partial Regex DatePatternRegex();

    [GeneratedRegex(@"\b\d{1,2}(?::\d{2})?\s*(?:am|pm|AM|PM)\b")]
    private static partial Regex TimePatternRegex();
}
