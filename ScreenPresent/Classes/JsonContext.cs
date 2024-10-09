using System.Collections.Generic;
using System.Text.Json.Serialization;
using static ScreenPresent.Classes.Version;

namespace ScreenPresent.Classes;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(JsonFile))]
[JsonSerializable(typeof(WeatherOneCallApiResult))]
[JsonSerializable(typeof(WeatherApiResult))]
[JsonSerializable(typeof(List<Versions>))]
public partial class JsonContext : JsonSerializerContext
{
}