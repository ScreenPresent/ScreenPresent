using System;
using System.Collections.Generic;

namespace ScreenPresent.Classes; 
public class WeatherResult(string cityName, ICollection<DailyWeather> dailyWeather)
{
    public string CityName { get; set; } = cityName;
    public ICollection<DailyWeather> DailyWeather { get; set; } = dailyWeather;
}

public class DailyWeather {
    public DayOfWeek WeekDay { get; set; }
    public required string IconId { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }
}