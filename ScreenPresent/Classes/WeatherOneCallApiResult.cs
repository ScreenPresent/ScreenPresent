using System.Text.Json.Serialization;

namespace ScreenPresent.Classes; 
/// <summary>
/// Fields in API response
/// </summary>
public class WeatherOneCallApiResult {
    /// <summary>
    /// Daily forecast weather data API response
    /// </summary>
    [JsonPropertyName("daily")]
    public required Daily[] Daily { get; set; }
}

public class Weather {
    /// <summary>
    /// Weather icon id. 
    /// </summary>
    [JsonPropertyName("icon")]
    public required string IconId { get; set; }
}

public class Daily {
    /// <summary>
    /// Time of the forecasted data, Unix, UTC
    /// </summary>
    [JsonPropertyName("dt")]
    public int DateStamp { get; set; }
    /// <summary>
    /// Units – default: kelvin, metric: Celsius, imperial: Fahrenheit.
    /// </summary>
    [JsonPropertyName("temp")]
    public required Temp Temperature { get; set; }
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("weather")]
    public required Weather[] Weather { get; set; }
}

public class Temp {
    /// <summary>
    /// Min daily temperature.
    /// </summary>
    [JsonPropertyName("min")]
    public float Min { get; set; }
    /// <summary>
    /// Max daily temperature.
    /// </summary>
    [JsonPropertyName("max")]
    public float Max { get; set; }
}
