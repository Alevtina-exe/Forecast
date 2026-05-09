using Forecast.Models;
using Forecast.Services;

public class WeatherManager
{
    private readonly IEnumerable<IWeatherProvider> _providers;
    private readonly Dictionary<string, (double Lat, double Lon)> _cityCoords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Минск", (53.90, 27.56) },
        { "Лондон", (51.50, -0.12) },
        { "Токио", (35.68, 139.69) },
        { "Шанхай", (31.23, 121.47) },
        { "Варшава", (52.23, 21.01) }
    };

    public WeatherManager(IEnumerable<IWeatherProvider> providers)
    {
        _providers = providers;
    }

    public async Task<WeatherResult> GetForecastAsync(string city, string providerName)
    {
        var provider = _providers.FirstOrDefault(p => p.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        if (provider == null) throw new ArgumentException("Provider not found");

        if (_cityCoords.TryGetValue(city, out var coords))
        {
            return await provider.GetWeatherAsync(coords.Lat, coords.Lon);
        }

        return await provider.GetWeatherByCityAsync(city);
    }
    public async Task<IEnumerable<WeatherResult>> GetWeatherForMultipleCitiesAsync(IEnumerable<string> cities, string providerName)
    {
        var tasks = cities.Select(city => GetForecastAsync(city, providerName)).ToList();
        return await Task.WhenAll(tasks);
    }
}