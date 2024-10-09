using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ScreenPresent.Converters;

public class HalfValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (((double)value) == 0)
        {
            return 0;
        }
        return (double)value / 2;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(value);
        return (double)value * 2;
    }
}