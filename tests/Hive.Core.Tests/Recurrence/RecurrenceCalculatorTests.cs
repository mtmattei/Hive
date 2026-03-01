using FluentAssertions;
using Hive.Core.Models;
using Hive.Core.Recurrence;
using Xunit;

namespace Hive.Core.Tests.Recurrence;

public class RecurrenceCalculatorTests
{
    private static readonly DateTimeOffset Jan1 = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Jan31 = new(2026, 1, 31, 23, 59, 59, TimeSpan.Zero);
    private static readonly DateTimeOffset Mar31 = new(2026, 3, 31, 23, 59, 59, TimeSpan.Zero);

    [Fact]
    public void Daily_Every1Day_GeneratesCorrectOccurrences()
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Frequency = RecurrenceFrequency.Daily,
            Interval = 1,
        };

        var result = RecurrenceCalculator.ExpandOccurrences(Jan1, rule, Jan1, Jan31);

        result.Should().HaveCount(31);
        result.First().Should().Be(Jan1);
        result.Last().Day.Should().Be(31);
    }

    [Fact]
    public void Daily_Every2Days_GeneratesCorrectOccurrences()
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Frequency = RecurrenceFrequency.Daily,
            Interval = 2,
        };

        var result = RecurrenceCalculator.ExpandOccurrences(Jan1, rule, Jan1, Jan31);

        result.Should().HaveCount(16); // Jan 1, 3, 5, ..., 29, 31
        result[1].Day.Should().Be(3);
    }

    [Fact]
    public void Weekly_SpecificDays_FiltersCorrectly()
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Frequency = RecurrenceFrequency.Weekly,
            Interval = 1,
            DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday],
        };

        var result = RecurrenceCalculator.ExpandOccurrences(Jan1, rule, Jan1, Jan31);

        result.Should().AllSatisfy(d =>
            d.DayOfWeek.Should().BeOneOf(DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday));
    }

    [Fact]
    public void Weekly_NoDaysSpecified_EveryWeek()
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Frequency = RecurrenceFrequency.Weekly,
            Interval = 1,
        };

        var result = RecurrenceCalculator.ExpandOccurrences(Jan1, rule, Jan1, Mar31);

        // 13 weeks from Jan 1 to Mar 31
        result.Should().HaveCountGreaterOrEqualTo(12);
        result.All(d => d.DayOfWeek == Jan1.DayOfWeek).Should().BeTrue();
    }

    [Fact]
    public void Monthly_GeneratesMonthly()
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Frequency = RecurrenceFrequency.Monthly,
            Interval = 1,
        };

        var rangeEnd = new DateTimeOffset(2026, 12, 31, 23, 59, 59, TimeSpan.Zero);
        var result = RecurrenceCalculator.ExpandOccurrences(Jan1, rule, Jan1, rangeEnd);

        result.Should().HaveCount(12);
        result.Select(d => d.Month).Should().BeInAscendingOrder();
    }

    [Fact]
    public void Yearly_GeneratesYearly()
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Frequency = RecurrenceFrequency.Yearly,
            Interval = 1,
        };

        var rangeEnd = new DateTimeOffset(2030, 12, 31, 23, 59, 59, TimeSpan.Zero);
        var result = RecurrenceCalculator.ExpandOccurrences(Jan1, rule, Jan1, rangeEnd);

        result.Should().HaveCount(5); // 2026-2030
    }

    [Fact]
    public void RepeatUntil_StopsAtEndDate()
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Frequency = RecurrenceFrequency.Daily,
            Interval = 1,
            RepeatUntil = new DateTimeOffset(2026, 1, 10, 23, 59, 59, TimeSpan.Zero),
        };

        var result = RecurrenceCalculator.ExpandOccurrences(Jan1, rule, Jan1, Jan31);

        result.Should().HaveCount(10);
        result.Last().Day.Should().Be(10);
    }

    [Fact]
    public void RangeStart_FiltersEarlierOccurrences()
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Frequency = RecurrenceFrequency.Daily,
            Interval = 1,
        };

        var rangeStart = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var result = RecurrenceCalculator.ExpandOccurrences(Jan1, rule, rangeStart, Jan31);

        result.Should().HaveCount(17); // Jan 15-31
        result.First().Day.Should().Be(15);
    }

    [Fact]
    public void ExpandEvent_NonRecurring_ReturnsSingle()
    {
        var evt = new CalendarEvent
        {
            Id = Guid.NewGuid(),
            Title = "One-time event",
            StartTime = Jan1,
            EndTime = Jan1.AddHours(1),
            Recurrence = null,
        };

        var result = RecurrenceCalculator.ExpandEvent(evt, Jan1, Jan31);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("One-time event");
    }

    [Fact]
    public void ExpandEvent_Recurring_PreservesDuration()
    {
        var evt = new CalendarEvent
        {
            Id = Guid.NewGuid(),
            Title = "Daily standup",
            StartTime = Jan1,
            EndTime = Jan1.AddMinutes(30),
            Recurrence = new RecurrenceRule
            {
                Id = Guid.NewGuid(),
                Frequency = RecurrenceFrequency.Daily,
                Interval = 1,
            },
        };

        var result = RecurrenceCalculator.ExpandEvent(evt, Jan1, Jan31);

        result.Should().HaveCount(31);
        result.Should().AllSatisfy(e =>
        {
            (e.EndTime!.Value - e.StartTime).Should().Be(TimeSpan.FromMinutes(30));
            e.Title.Should().Be("Daily standup");
        });
    }

    [Fact]
    public void MaxOccurrences_Caps()
    {
        var rule = new RecurrenceRule
        {
            Id = Guid.NewGuid(),
            Frequency = RecurrenceFrequency.Daily,
            Interval = 1,
        };

        var farFuture = Jan1.AddYears(10);
        var result = RecurrenceCalculator.ExpandOccurrences(Jan1, rule, Jan1, farFuture, maxOccurrences: 100);

        result.Should().HaveCount(100);
    }
}
