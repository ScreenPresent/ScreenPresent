using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ScreenPresent.Converters;

internal class TemperatureConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(value);
        return double.Parse(value.ToString()!).ToString("N1") + "°C";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException($"The method {nameof(ConvertBack)} in {nameof(TemperatureConverter)} is not implemented.");
    }
}
