namespace Hive.Core.Services;

public interface IWeatherService
{
    Task<WeatherForecast?> GetForecastAsync(string location, DateTimeOffset date, CancellationToken ct = default);
    Task<WeatherForecast?> GetCurrentAsync(string location, CancellationToken ct = default);
}

public record WeatherForecast(
    string Condition,
    string Icon,
    int TempHighF,
    int TempLowF,
    int TempCurrentF,
    int HumidityPercent,
    string Summary);
