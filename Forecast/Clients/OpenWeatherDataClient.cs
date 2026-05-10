using Forecast.Models;
using Forecast.Utils;
using System.Text.Json.Serialization;

namespace Forecast.Clients;

public class OpenWeatherDataClient : IWeatherDataClient
{
    private readonly HttpClient _httpClient; 
    private readonly string _apiKey;

    public string ProviderName => "OpenWeather";

    public OpenWeatherDataClient(IConfiguration config, HttpClient httpClient)
    {
        _httpClient = httpClient; 

        var baseUrl = config.GetValue<string>("OPENWEATHER_BASE_URL");
        if (!string.IsNullOrEmpty(baseUrl))
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        _apiKey = config.GetValue<string>("OPENWEATHER_API_KEY") ?? "";
    }

    public async Task<decimal> LocationCurrentTemperature(decimal latitude, decimal longitude)
    {
        try
        {

            var response = await _httpClient.GetAsync(
                $"?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric"
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiCallException($"openweather returned bad status: {(ushort)response.StatusCode}");
            }

            var data = await response.Content.ReadFromJsonAsync<OpenWeatherResponse>();
            return data?.Main?.Temp ?? throw new ApiCallException("failed to decode response");
        }
        catch (HttpRequestException e)
        {
            throw new ApiCallException($"failed to call openweather: {e.Message}.", inner: e);
        }
    }

    public async Task<decimal> CityCurrentTemperature(string cityName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"?q={cityName}&appid={_apiKey}&units=metric");

            if (!response.IsSuccessStatusCode)
                throw new ApiCallException($"City '{cityName}' not found.");

            var data = await response.Content.ReadFromJsonAsync<OpenWeatherResponse>();
            return data?.Main?.Temp ?? throw new ApiCallException("Failed to decode");
        }
        catch (HttpRequestException e)
        {
            throw new ApiCallException(e.Message);
        }
    }

    public async Task<WeatherForecast> GetWeatherForecastAsync(string location)
    {
        string url;

        if (location.Contains(','))
        {
            var parts = location.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var lat = parts[0].Trim();
            var lon = parts[1].Trim();
            url = $"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
        }
        else
        {
            url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&appid={_apiKey}&units=metric";
        }

        var response = await _httpClient.GetFromJsonAsync<OpenWeatherForecastRaw>(url);

        if (response?.List == null)
            throw new Exception("Не удалось получить прогноз от OpenWeather");

        return new WeatherForecast
        {
            City = response.City?.Name ?? location,
            Items = response.List.Select(i => new ForecastItem
            {
                DateTime = DateTimeOffset.FromUnixTimeSeconds(i.Dt).DateTime.ToLocalTime(),
                Temperature = i.Main?.Temp ?? 0,
                Condition = i.Weather?.FirstOrDefault()?.Main ?? "Unknown",
                PrecipitationProbability = (int)((i.Pop ?? 0) * 100)
            }).ToList()
        };
    }
}


