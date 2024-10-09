using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ScreenPresent.Converters;

class WeekdayConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(value);
        return (DayOfWeek)value switch
        {
            DayOfWeek.Sunday => "Sonntag",
            DayOfWeek.Monday => "Montag",
            DayOfWeek.Tuesday => "Dienstag",
            DayOfWeek.Wednesday => "Mittwoch",
            DayOfWeek.Thursday => "Donnerstag",
            DayOfWeek.Friday => "Freitag",
            DayOfWeek.Saturday => "Samstag",
            _ => throw new NotImplementedException($"Unkown value: {value}"),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException($"The method {nameof(ConvertBack)} in {nameof(WeekdayConverter)} is not implemented.");
    }
}