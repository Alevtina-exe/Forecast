using System.Text.Json.Serialization;

namespace Forecast.Models.Google;

public class GoogleWeatherResponse
{
    [JsonPropertyName("temperature")]
    public required TemperatureData Temperature { get; set; }

    [JsonPropertyName("currentTime")]
    public string? CurrentTime { get; set; }
}

public class TemperatureData
{
    [JsonPropertyName("degrees")]
    public decimal Degrees { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
}