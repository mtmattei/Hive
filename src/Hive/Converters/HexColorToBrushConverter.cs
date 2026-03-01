using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Hive.Converters;

/// <summary>
/// Converts a hex color string (e.g. "#C2553A") to a SolidColorBrush.
/// </summary>
public class HexColorToBrushConverter : IValueConverter
{
    public double Opacity { get; set; } = 1.0;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string hex && hex.StartsWith('#') && hex.Length >= 7)
        {
            var r = System.Convert.ToByte(hex.Substring(1, 2), 16);
            var g = System.Convert.ToByte(hex.Substring(3, 2), 16);
            var b = System.Convert.ToByte(hex.Substring(5, 2), 16);
            var a = (byte)(255 * Opacity);

            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts a hex color string to a tinted background (color at ~7% opacity).
/// Used for event card backgrounds.
/// </summary>
public class HexColorToTintConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string hex && hex.StartsWith('#') && hex.Length >= 7)
        {
            var r = System.Convert.ToByte(hex.Substring(1, 2), 16);
            var g = System.Convert.ToByte(hex.Substring(3, 2), 16);
            var b = System.Convert.ToByte(hex.Substring(5, 2), 16);

            return new SolidColorBrush(Color.FromArgb(18, r, g, b)); // ~7% opacity
        }

        return new SolidColorBrush(Color.FromArgb(18, 128, 128, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
