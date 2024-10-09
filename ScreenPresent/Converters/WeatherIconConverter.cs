using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ScreenPresent.Converters;

public class WeatherIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(value);
        var iconID = value.ToString();
        if (iconID == null) return null!;
        return $"https://openweathermap.org/img/wn/{iconID}@2x.png";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
