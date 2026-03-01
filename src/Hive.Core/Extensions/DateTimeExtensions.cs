namespace Hive.Core.Extensions;

public static class DateTimeExtensions
{
    public static DateTimeOffset StartOfDay(this DateTimeOffset dt) =>
        new(dt.Year, dt.Month, dt.Day, 0, 0, 0, dt.Offset);

    public static DateTimeOffset EndOfDay(this DateTimeOffset dt) =>
        new(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999, dt.Offset);

    public static DateTimeOffset StartOfWeek(this DateTimeOffset dt, DayOfWeek startDay = DayOfWeek.Sunday)
    {
        var diff = (7 + (dt.DayOfWeek - startDay)) % 7;
        return dt.AddDays(-diff).StartOfDay();
    }

    public static DateTimeOffset EndOfWeek(this DateTimeOffset dt, DayOfWeek startDay = DayOfWeek.Sunday) =>
        dt.StartOfWeek(startDay).AddDays(6).EndOfDay();

    public static DateTimeOffset StartOfMonth(this DateTimeOffset dt) =>
        new(dt.Year, dt.Month, 1, 0, 0, 0, dt.Offset);

    public static DateTimeOffset EndOfMonth(this DateTimeOffset dt) =>
        new(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month), 23, 59, 59, 999, dt.Offset);

    public static DateOnly ToDateOnly(this DateTimeOffset dt) =>
        DateOnly.FromDateTime(dt.DateTime);
}
