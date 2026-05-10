using System.Text.Json.Serialization;

namespace Forecast.Models
{
    class OpenWeatherResponse
    {
        [JsonPropertyName("main")]
        public required Nested Main { get; set; }

        public class Nested
        {
            [JsonPropertyName("temp")]
            public decimal Temp { get; set; }
        }
    }
    public class OpenWeatherForecastRaw
    {
        [JsonPropertyName("list")]
        public List<ForecastPoint>? List { get; set; }

        [JsonPropertyName("city")]
        public CityInfo? City { get; set; }
    }

    public class ForecastPoint
    {
        [JsonPropertyName("dt")]
        public long Dt { get; set; } // Unix timestamp

        [JsonPropertyName("main")]
        public MainData? Main { get; set; }

        [JsonPropertyName("weather")]
        public List<WeatherDescription>? Weather { get; set; }

        [JsonPropertyName("pop")]
        public double? Pop { get; set; } // Probability of precipitation (0 to 1)
    }

    public class MainData
    {
        [JsonPropertyName("temp")]
        public decimal Temp { get; set; }
    }

    public class WeatherDescription
    {
        [JsonPropertyName("main")]
        public string? Main { get; set; } 
    }

    public class CityInfo
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
