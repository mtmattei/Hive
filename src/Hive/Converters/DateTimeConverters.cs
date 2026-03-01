using Microsoft.UI.Xaml.Data;

namespace Hive.Converters;

public class TimeFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            DateTimeOffset dto => dto.ToString("h:mm tt"),
            DateTime dt => dt.ToString("h:mm tt"),
            TimeOnly t => t.ToString("h:mm tt"),
            _ => string.Empty,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}

public class DateFormatConverter : IValueConverter
{
    public string Format { get; set; } = "MMM d";

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            DateTimeOffset dto => dto.ToString(Format),
            DateTime dt => dt.ToString(Format),
            DateOnly d => d.ToString(Format),
            _ => string.Empty,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
