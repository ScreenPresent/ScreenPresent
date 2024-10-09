namespace ScreenPresent.Classes; 
public class WeatherApiResult {
    public required Coord coord { get; set; }
}

public class Coord {
    public float lon { get; set; }
    public float lat { get; set; }
}