namespace Forecast.Services;

public class WeatherManager
{
    private readonly IEnumerable<IWeatherProvider> _providers;

    public WeatherManager(IEnumerable<IWeatherProvider> providers)
    {
        _providers = providers;
    }

    public async Task GetForecastAsync(string city, string providerName)
    {
        throw new NotImplementedException();
    }
}