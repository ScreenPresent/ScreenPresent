using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace ScreenPresent.Converters;

public class BoolToStretchConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(value);
        return (bool)value ? Stretch.Fill : Stretch.Uniform;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(value);
        return (Stretch)value == Stretch.Fill;
    }
}