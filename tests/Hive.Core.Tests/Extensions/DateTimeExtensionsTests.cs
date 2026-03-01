using FluentAssertions;
using Hive.Core.Extensions;
using Xunit;

namespace Hive.Core.Tests.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void StartOfDay_ReturnsCorrectTime()
    {
        var dt = new DateTimeOffset(2026, 3, 15, 14, 30, 45, TimeSpan.FromHours(-5));
        var result = dt.StartOfDay();

        result.Hour.Should().Be(0);
        result.Minute.Should().Be(0);
        result.Second.Should().Be(0);
        result.Day.Should().Be(15);
    }

    [Fact]
    public void EndOfDay_ReturnsCorrectTime()
    {
        var dt = new DateTimeOffset(2026, 3, 15, 14, 30, 45, TimeSpan.FromHours(-5));
        var result = dt.EndOfDay();

        result.Hour.Should().Be(23);
        result.Minute.Should().Be(59);
        result.Second.Should().Be(59);
    }

    [Fact]
    public void StartOfWeek_Sunday_ReturnsCorrect()
    {
        // March 15, 2026 is a Sunday
        var dt = new DateTimeOffset(2026, 3, 18, 12, 0, 0, TimeSpan.Zero); // Wednesday
        var result = dt.StartOfWeek(DayOfWeek.Sunday);

        result.DayOfWeek.Should().Be(DayOfWeek.Sunday);
        result.Day.Should().Be(15);
    }

    [Fact]
    public void StartOfWeek_Monday_ReturnsCorrect()
    {
        var dt = new DateTimeOffset(2026, 3, 18, 12, 0, 0, TimeSpan.Zero); // Wednesday
        var result = dt.StartOfWeek(DayOfWeek.Monday);

        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.Day.Should().Be(16);
    }

    [Fact]
    public void EndOfWeek_ReturnsCorrect()
    {
        var dt = new DateTimeOffset(2026, 3, 18, 12, 0, 0, TimeSpan.Zero); // Wednesday
        var result = dt.EndOfWeek(DayOfWeek.Sunday);

        result.DayOfWeek.Should().Be(DayOfWeek.Saturday);
        result.Day.Should().Be(21);
    }

    [Fact]
    public void StartOfMonth_ReturnsFirstDay()
    {
        var dt = new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var result = dt.StartOfMonth();

        result.Day.Should().Be(1);
        result.Month.Should().Be(3);
    }

    [Fact]
    public void EndOfMonth_ReturnsLastDay()
    {
        var dt = new DateTimeOffset(2026, 2, 10, 12, 0, 0, TimeSpan.Zero);
        var result = dt.EndOfMonth();

        result.Day.Should().Be(28); // 2026 is not a leap year
        result.Month.Should().Be(2);
    }

    [Fact]
    public void ToDateOnly_ConvertsCorrectly()
    {
        var dt = new DateTimeOffset(2026, 6, 15, 14, 30, 0, TimeSpan.FromHours(-5));
        var result = dt.ToDateOnly();

        result.Year.Should().Be(2026);
        result.Month.Should().Be(6);
        result.Day.Should().Be(15);
    }
}
