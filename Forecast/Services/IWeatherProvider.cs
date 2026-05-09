using Forecast.Models;
namespace Forecast.Services;

public interface IWeatherProvider
{
    string Name { get; }
    Task<WeatherResult> GetWeatherAsync(double lat, double lon);
    Task<WeatherResult> GetWeatherByCityAsync(string cityName);
}
