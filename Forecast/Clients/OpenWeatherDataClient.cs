using System.Text.Json.Serialization;
using Forecast.Utils;

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
}

class OpenWeatherResponse
{
    [JsonPropertyName("main")]
    public required Nested Main { get; set; }

    public class Nested
    {
        [JsonPropertyName("temp")]
        public decimal Temp { get; set; }
    }
}
