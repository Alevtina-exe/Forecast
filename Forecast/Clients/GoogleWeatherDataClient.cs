namespace Forecast.Clients;

public class GoogleWeatherDataClient : IWeatherDataClient
{
    private readonly HttpClient _httpClient;

    public GoogleWeatherDataClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string ProviderName => "Google";

    public Task<decimal> LocationCurrentTemperature(decimal latitude, decimal longitude)
    {
        return Task.FromResult(25.0m);
    }

    public Task<decimal> CityCurrentTemperature(string cityName)
    {
        return Task.FromResult(22.5m);
    }
}