using Hive.Core.Services;

namespace Hive.Services;

/// <summary>
/// Weather service using Open-Meteo free API (no key required).
/// Provides weather forecasts for event locations.
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly HttpClient _http = new();

    public async Task<WeatherForecast?> GetCurrentAsync(string location, CancellationToken ct = default)
    {
        return await GetForecastAsync(location, DateTimeOffset.Now, ct);
    }

    public async Task<WeatherForecast?> GetForecastAsync(
        string location, DateTimeOffset date, CancellationToken ct = default)
    {
        try
        {
            // Use Open-Meteo geocoding to get coordinates from location name
            var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(location)}&count=1";
            var geoResponse = await _http.GetStringAsync(geoUrl, ct);

            // Simple JSON parsing without System.Text.Json dependency issues
            var latIdx = geoResponse.IndexOf("\"latitude\":", StringComparison.Ordinal);
            var lonIdx = geoResponse.IndexOf("\"longitude\":", StringComparison.Ordinal);
            if (latIdx < 0 || lonIdx < 0) return null;

            var lat = ExtractNumber(geoResponse, latIdx + 11);
            var lon = ExtractNumber(geoResponse, lonIdx + 12);

            // Get weather forecast
            var dateStr = date.ToString("yyyy-MM-dd");
            var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}"
                + $"&daily=temperature_2m_max,temperature_2m_min,weathercode"
                + $"&current=temperature_2m,relative_humidity_2m,weathercode"
                + $"&temperature_unit=fahrenheit&start_date={dateStr}&end_date={dateStr}&timezone=auto";

            var weatherResponse = await _http.GetStringAsync(weatherUrl, ct);

            var tempHigh = (int)ExtractJsonNumber(weatherResponse, "temperature_2m_max", true);
            var tempLow = (int)ExtractJsonNumber(weatherResponse, "temperature_2m_min", true);
            var tempCurrent = (int)ExtractJsonNumber(weatherResponse, "\"temperature_2m\":", false);
            var humidity = (int)ExtractJsonNumber(weatherResponse, "relative_humidity_2m", false);
            var code = (int)ExtractJsonNumber(weatherResponse, "\"weathercode\":", false);

            var (condition, icon) = MapWeatherCode(code);

            return new WeatherForecast(
                Condition: condition,
                Icon: icon,
                TempHighF: tempHigh,
                TempLowF: tempLow,
                TempCurrentF: tempCurrent,
                HumidityPercent: humidity,
                Summary: $"{condition}, {tempHigh}°F / {tempLow}°F");
        }
        catch
        {
            return null;
        }
    }

    private static double ExtractNumber(string json, int startIdx)
    {
        var end = startIdx;
        while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '.' || json[end] == '-'))
            end++;
        return double.TryParse(json[startIdx..end], System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var val) ? val : 0;
    }

    private static double ExtractJsonNumber(string json, string key, bool inArray)
    {
        var idx = json.IndexOf(key, StringComparison.Ordinal);
        if (idx < 0) return 0;
        idx += key.Length;
        // Skip to the value
        while (idx < json.Length && !char.IsDigit(json[idx]) && json[idx] != '-')
            idx++;
        return ExtractNumber(json, idx);
    }

    private static (string Condition, string Icon) MapWeatherCode(int code) => code switch
    {
        0 => ("Clear sky", "\u2600\uFE0F"),
        1 => ("Mainly clear", "\U0001F324\uFE0F"),
        2 => ("Partly cloudy", "\u26C5"),
        3 => ("Overcast", "\u2601\uFE0F"),
        45 or 48 => ("Foggy", "\U0001F32B\uFE0F"),
        51 or 53 or 55 => ("Drizzle", "\U0001F326\uFE0F"),
        61 or 63 or 65 => ("Rain", "\U0001F327\uFE0F"),
        66 or 67 => ("Freezing rain", "\U0001F327\uFE0F"),
        71 or 73 or 75 => ("Snow", "\U0001F328\uFE0F"),
        77 => ("Snow grains", "\u2744\uFE0F"),
        80 or 81 or 82 => ("Showers", "\U0001F326\uFE0F"),
        85 or 86 => ("Snow showers", "\U0001F328\uFE0F"),
        95 => ("Thunderstorm", "\u26C8\uFE0F"),
        96 or 99 => ("Thunderstorm with hail", "\u26C8\uFE0F"),
        _ => ("Unknown", "\U0001F300"),
    };
}
