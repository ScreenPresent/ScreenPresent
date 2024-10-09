using System.Text.Json.Serialization;

namespace ScreenPresent.Classes;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(JsonFile))]
[JsonSerializable(typeof(WeatherOneCallApiResult))]
[JsonSerializable(typeof(WeatherApiResult))]
public partial class JsonContext : JsonSerializerContext
{
}