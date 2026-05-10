using Forecast.Clients;
using Forecast.Models.Weather;
using Microsoft.AspNetCore.Mvc;
using Sprache;
using System.Globalization;

namespace Forecast.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrentWeatherController : ControllerBase
{
    private readonly IEnumerable<IWeatherDataClient> _clients;

    public CurrentWeatherController(IEnumerable<IWeatherDataClient> clients)
    {
        _clients = clients;
    }

    public async Task<CurrentWeather> GetCurrentWeather(string location, string providerName)
    {
        var client = _clients.FirstOrDefault(c =>
            c.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (client == null)
        {
            throw new ArgumentException($"Провайдер '{providerName}' не найден.");
        }

        decimal temp = 0;

        if (location.Contains(','))
        {
            var parts = location.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
            {
                string latStr = parts[0].Trim().Replace(',', '.');
                string lonStr = parts[1].Trim().Replace(',', '.');

                decimal lat = decimal.Parse(latStr, CultureInfo.InvariantCulture);
                decimal lon = decimal.Parse(lonStr, CultureInfo.InvariantCulture);

                temp = await client.LocationCurrentTemperature(lat, lon);
            }
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