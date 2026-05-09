using Forecast.Clients;
using Forecast.Models.Weather;
using Sprache;
using System.Globalization;

namespace Forecast.Controllers;

public class CurrentWeatherController
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

    public async Task<IEnumerable<CurrentWeather>> GetWeatherBatch(IEnumerable<string> locations, string providerName)
    {
        var tasks = locations.Select(loc => GetCurrentWeather(loc, providerName));

        return await Task.WhenAll(tasks);
    }

}