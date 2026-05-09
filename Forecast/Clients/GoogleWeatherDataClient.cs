namespace Forecast.Clients;

public class GoogleWeatherDataClient : IWeatherDataClient
{
    public string ProviderName => "Google";

    public Task<decimal> LocationCurrentTemperature(decimal latitude, decimal longitude)
    {
        return Task.FromResult(25.0m);
    }

    public Task<decimal> CityCurrentTemperature(string cityName)
    {
        return Task.FromResult(20.0m);
    }
}