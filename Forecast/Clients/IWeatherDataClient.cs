namespace Forecast.Clients;

public interface IWeatherDataClient
{
    string ProviderName { get; }
    Task<decimal> LocationCurrentTemperature(decimal latitude, decimal longitude);
    Task<decimal> CityCurrentTemperature(string cityName);
}