using Forecast.Clients;
using Forecast.Models.Weather;

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

        decimal temp;
        if (IsCoordinates(location))
        {
            var parts = location.Split(',');
            temp = await client.LocationCurrentTemperature(decimal.Parse(parts[0]), decimal.Parse(parts[1]));
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

    private bool IsCoordinates(string input) => input.Contains(',');
}