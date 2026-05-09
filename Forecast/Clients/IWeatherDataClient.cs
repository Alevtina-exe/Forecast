namespace Forecast.Clients;

interface IWeatherDataClient
{
    Task<decimal> LocationCurrentTemperature(decimal latitude, decimal longitude);
}
