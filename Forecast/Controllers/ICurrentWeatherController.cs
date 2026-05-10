using Forecast.Models;

public interface ICurrentWeatherController
{
    Task<decimal> GetCurrentWeather(string location, string provider);
    Task<IEnumerable<object>> GetWeatherBatch(string[] locations, string provider);
    Task<WeatherForecast> GetWeatherForecast(string location, string provider);
}