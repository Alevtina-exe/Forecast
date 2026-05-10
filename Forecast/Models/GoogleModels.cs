using System.Text.Json.Serialization;

namespace Forecast.Models;

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

public class GoogleForecastRawResponse
{
    [JsonPropertyName("forecastHours")]
    public List<GoogleForecastHour>? ForecastHours { get; set; }
}

public class GoogleForecastHour
{
    [JsonPropertyName("interval")]
    public GoogleInterval? Interval { get; set; }

    [JsonPropertyName("weatherCondition")]
    public GoogleWeatherCondition? WeatherCondition { get; set; }

    [JsonPropertyName("temperature")]
    public GoogleTemp? Temperature { get; set; }

    [JsonPropertyName("precipitation")]
    public GooglePrecipitation? Precipitation { get; set; }
}

public class GoogleInterval
{
    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }
}

public class GoogleWeatherCondition
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class GoogleTemp
{
    [JsonPropertyName("degrees")]
    public decimal Degrees { get; set; }
}

public class GooglePrecipitation
{
    [JsonPropertyName("probability")]
    public GoogleProbability? Probability { get; set; }
}

public class GoogleProbability
{
    [JsonPropertyName("percent")]
    public int Percent { get; set; }
}