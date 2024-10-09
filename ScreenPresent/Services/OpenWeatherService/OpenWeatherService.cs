using ScreenPresent.Classes;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScreenPresent.Services.OpenWeatherService;
public class OpenWeatherMapService : IWeatherService
{
    private readonly HttpClient client;
    private float _lat;
    private float _lon;

    public OpenWeatherMapService()
    {
        client = new HttpClient
        {
            BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/")
        };
    }

    public async Task<WeatherResult> GetForecastAsync(string apiKey, string location)
    {
        try
        {
            await GetCoords(location, apiKey);

            string query = "onecall?" +
                "exclude=hourly,minutely" +
                $"&appid={apiKey}" +
                $"&lat={_lat}" +
                $"&lon={_lon}" +
                "&units=metric" +
                "&lang=de";
            HttpResponseMessage response = await client.GetAsync(query);

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new Exceptions.UnauthorizedApiAccessException("Invalid API key.");
                case HttpStatusCode.NotFound:
                    throw new Exceptions.LocationNotFoundException("Location not found.");
                case HttpStatusCode.OK:
                    string result = await response.Content.ReadAsStringAsync();
                    WeatherOneCallApiResult? weatherOneCallApiResult = JsonSerializer.Deserialize(result, typeof(WeatherOneCallApiResult), JsonContext.Default) as WeatherOneCallApiResult;
                    if (weatherOneCallApiResult == null)
                    {
                        throw new InvalidOperationException("The wather response could not be parsed.");
                    }
                    var weatherResult = new WeatherResult(location, weatherOneCallApiResult.Daily.Select(daily =>
                           new DailyWeather()
                           {
                               WeekDay = DateTimeOffset.FromUnixTimeSeconds(daily.DateStamp).DayOfWeek,
                               IconId = daily.Weather.First().IconId,
                               Max = daily.Temperature.Max,
                               Min = daily.Temperature.Min
                           }
                        ).ToList()
                    );
                    return weatherResult;
                default:
                    throw new NotImplementedException(response.StatusCode.ToString());
            }
        }
        catch
        {
            string result = "{\"lat\":52.85,\"lon\":7.52,\"timezone\":\"Europe/Berlin\",\"timezone_offset\":7200,\"current\":{\"dt\":1600421727,\"sunrise\":1600405772,\"sunset\":1600450712,\"temp\":14.01,\"feels_like\":11.67,\"pressure\":1029,\"humidity\":66,\"dew_point\":7.77,\"uvi\":3.62,\"clouds\":20,\"IsVisible\":10000,\"wind_speed\":2.6,\"wind_deg\":80,\"weather\":[{\"id\":801,\"main\":\"Clouds\",\"description\":\"Ein paar Wolken\",\"icon\":\"02d\"}]},\"daily\":[{\"dt\":1600426800,\"sunrise\":1600405772,\"sunset\":1600450712,\"temp\":{\"day\":15.54,\"min\":6.95,\"max\":16.53,\"night\":10.34,\"eve\":12.32,\"morn\":6.95},\"feels_like\":{\"day\":10.9,\"night\":5.87,\"eve\":8.67,\"morn\":2.88},\"pressure\":1029,\"humidity\":60,\"dew_point\":7.82,\"wind_speed\":5.9,\"wind_deg\":91,\"weather\":[{\"id\":801,\"main\":\"Clouds\",\"description\":\"Ein paar Wolken\",\"icon\":\"02d\"}],\"clouds\":14,\"pop\":0,\"uvi\":3.62},{\"dt\":1600513200,\"sunrise\":1600492273,\"sunset\":1600536966,\"temp\":{\"day\":18.17,\"min\":8.38,\"max\":18.41,\"night\":10.45,\"eve\":12.63,\"morn\":8.38},\"feels_like\":{\"day\":13.41,\"night\":6.57,\"eve\":8.99,\"morn\":3.55},\"pressure\":1021,\"humidity\":52,\"dew_point\":8.35,\"wind_speed\":6.19,\"wind_deg\":85,\"weather\":[{\"id\":800,\"main\":\"Clear\",\"description\":\"Klarer Himmel\",\"icon\":\"01d\"}],\"clouds\":10,\"pop\":0,\"uvi\":3.35},{\"dt\":1600599600,\"sunrise\":1600578775,\"sunset\":1600623221,\"temp\":{\"day\":18.45,\"min\":8.01,\"max\":18.93,\"night\":10.65,\"eve\":12.64,\"morn\":8.17},\"feels_like\":{\"day\":15.35,\"night\":7.69,\"eve\":10.1,\"morn\":4.49},\"pressure\":1019,\"humidity\":58,\"dew_point\":10.17,\"wind_speed\":4.5,\"wind_deg\":58,\"weather\":[{\"id\":802,\"main\":\"Clouds\",\"description\":\"Mäßig bewölkt\",\"icon\":\"03d\"}],\"clouds\":43,\"pop\":0,\"uvi\":3.19},{\"dt\":1600686000,\"sunrise\":1600665276,\"sunset\":1600709476,\"temp\":{\"day\":20.06,\"min\":8.88,\"max\":20.9,\"night\":13.19,\"eve\":14.88,\"morn\":8.88},\"feels_like\":{\"day\":18.52,\"night\":11.96,\"eve\":13.98,\"morn\":6.14},\"pressure\":1015,\"humidity\":52,\"dew_point\":10.05,\"wind_speed\":2.23,\"wind_deg\":106,\"weather\":[{\"id\":800,\"main\":\"Clear\",\"description\":\"Klarer Himmel\",\"icon\":\"01d\"}],\"clouds\":0,\"pop\":0,\"uvi\":2.94},{\"dt\":1600772400,\"sunrise\":1600751777,\"sunset\":1600795730,\"temp\":{\"day\":20.78,\"min\":11.32,\"max\":20.78,\"night\":14.75,\"eve\":15.48,\"morn\":11.32},\"feels_like\":{\"day\":16.61,\"night\":13.33,\"eve\":13.29,\"morn\":8.65},\"pressure\":1006,\"humidity\":53,\"dew_point\":10.91,\"wind_speed\":6.36,\"wind_deg\":237,\"weather\":[{\"id\":800,\"main\":\"Clear\",\"description\":\"Klarer Himmel\",\"icon\":\"01d\"}],\"clouds\":0,\"pop\":0.06,\"uvi\":3.06},{\"dt\":1600858800,\"sunrise\":1600838279,\"sunset\":1600881985,\"temp\":{\"day\":15.17,\"min\":11,\"max\":15.17,\"night\":11,\"eve\":12.91,\"morn\":12.9},\"feels_like\":{\"day\":11.64,\"night\":9.37,\"eve\":10.39,\"morn\":13.09},\"pressure\":1004,\"humidity\":87,\"dew_point\":13.03,\"wind_speed\":6.38,\"wind_deg\":204,\"weather\":[{\"id\":501,\"main\":\"Rain\",\"description\":\"Mäßiger Regen\",\"icon\":\"10d\"}],\"clouds\":100,\"pop\":1,\"rain\":6.63,\"uvi\":3.05},{\"dt\":1600945200,\"sunrise\":1600924780,\"sunset\":1600968240,\"temp\":{\"day\":14.25,\"min\":8.72,\"max\":14.25,\"night\":8.72,\"eve\":9.52,\"morn\":11.84},\"feels_like\":{\"day\":7.99,\"night\":4.71,\"eve\":7.76,\"morn\":7.12},\"pressure\":993,\"humidity\":66,\"dew_point\":8.08,\"wind_speed\":8.27,\"wind_deg\":209,\"weather\":[{\"id\":501,\"main\":\"Rain\",\"description\":\"Mäßiger Regen\",\"icon\":\"10d\"}],\"clouds\":100,\"pop\":1,\"rain\":3.12,\"uvi\":2.87},{\"dt\":1601031600,\"sunrise\":1601011282,\"sunset\":1601054495,\"temp\":{\"day\":12.77,\"min\":7.82,\"max\":13.11,\"night\":8.56,\"eve\":9.48,\"morn\":8.45},\"feels_like\":{\"day\":6.79,\"night\":3.89,\"eve\":5.26,\"morn\":2.86},\"pressure\":987,\"humidity\":76,\"dew_point\":8.67,\"wind_speed\":8.1,\"wind_deg\":237,\"weather\":[{\"id\":501,\"main\":\"Rain\",\"description\":\"Mäßiger Regen\",\"icon\":\"10d\"}],\"clouds\":85,\"pop\":1,\"rain\":4.54,\"uvi\":2.47}]}";
            if (JsonSerializer.Deserialize(result, typeof(WeatherOneCallApiResult), JsonContext.Default) is not WeatherOneCallApiResult weatherOneCallApiResult)
            {
                throw new InvalidOperationException("The default value could not be loaded.");
            }
            return new WeatherResult(location, weatherOneCallApiResult.Daily.Select(daily =>
                   new DailyWeather()
                   {
                       WeekDay = DateTimeOffset.FromUnixTimeSeconds(daily.DateStamp).DayOfWeek,
                       IconId = daily.Weather.First().IconId,
                       Max = daily.Temperature.Max,
                       Min = daily.Temperature.Min
                   }
                ).ToList()
            );
        }
    }

    private Task GetCoords(string location, string apiKey)
    {
        if (_lat != 0 && _lon != 0)
        {
            throw new ArgumentException("Coords can't be empty.");
        }
        if (string.IsNullOrEmpty(location))
        {
            throw new ArgumentException("Location can't be an empty string.");
        }
        return GetCoordsInternal(location, apiKey);
    }

    private async Task GetCoordsInternal(string location, string apiKey)
    {
        string query = $"weather?" +
            $"q={location}" +
            $"&appid={apiKey}";

        HttpResponseMessage response = await client.GetAsync(query);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                throw new Exceptions.UnauthorizedApiAccessException("Invalid API key.");
            case HttpStatusCode.NotFound:
                throw new Exceptions.LocationNotFoundException("Location not found.");
            case HttpStatusCode.OK:
                string result = await response.Content.ReadAsStringAsync();
                WeatherApiResult? weatherApiResult = JsonSerializer.Deserialize(result, typeof(WeatherApiResult), JsonContext.Default) as WeatherApiResult;
                if (weatherApiResult == null)
                {
                    throw new InvalidOperationException("The coords could not be loaded.");
                }
                _lat = weatherApiResult.coord.lat;
                _lon = weatherApiResult.coord.lon;
                break;
            default:
                throw new NotImplementedException(response.StatusCode.ToString());
        }

    }
}
