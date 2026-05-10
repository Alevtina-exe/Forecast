using Forecast.Clients;
using Forecast.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Forecast.Controllers;
public class CurrentWeatherController : ControllerBase
{
    private readonly IEnumerable<IWeatherDataClient> _clients;

    public CurrentWeatherController(IEnumerable<IWeatherDataClient> clients)
    {
        _clients = clients;
    }

    private IWeatherDataClient GetClient(string providerName)
    {
        return _clients.FirstOrDefault(c =>
            c.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Провайдер '{providerName}' не найден.");
    }
    public async Task<WeatherForecast> GetWeatherForecast(string location, string providerName)
    {
        var client = GetClient(providerName);
        return await client.GetWeatherForecastAsync(location);
    }

    public async Task<CurrentWeather> GetCurrentWeather(string location, string providerName)
    {
        var client = GetClient(providerName);
        decimal temp;

        if (location.Contains(','))
        {
            var parts = location.Split(',', StringSplitOptions.RemoveEmptyEntries);
            decimal lat = decimal.Parse(parts[0], CultureInfo.InvariantCulture);
            decimal lon = decimal.Parse(parts[1], CultureInfo.InvariantCulture);
            temp = await client.LocationCurrentTemperature(lat, lon);
        }
        else
        {
            temp = await client.CityCurrentTemperature(location);
        }

        return new CurrentWeather(temp);
    }

    [HttpGet("batch")]
    public async Task<IActionResult> GetWeatherBatch([FromQuery] string[] locations, [FromQuery] string providerName)
    {
        var tasks = locations.Select(async loc =>
        {
            try
            {
                var weather = await GetCurrentWeather(loc, providerName);
                return new { Location = loc, Temperature = weather.Temperature, Status = "Success" };
            }
            catch (Exception ex)
            {
                return new { Location = loc, Temperature = 0m, Status = $"Error: {ex.Message}" };
            }
        });

        var results = await Task.WhenAll(tasks);
        return Ok(results);
    }
}