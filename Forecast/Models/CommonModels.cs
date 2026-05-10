namespace Forecast.Models
{
    public record CurrentWeather(decimal Temperature);


    public class WeatherForecast
    {
        public string City { get; set; }
        public List<ForecastItem> Items { get; set; } = new();
    }

    public class ForecastItem
    {
        public DateTime DateTime { get; set; }
        public decimal Temperature { get; set; }
        public string Condition { get; set; }
        public int PrecipitationProbability { get; set; }
    }
}
