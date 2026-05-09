using System.Text.Json.Serialization;
using Forecast.Utils;

namespace Forecast.Clients;

class OpenWeatherDataClient : IWeatherDataClient
{
    private readonly HttpClient client;
    private readonly string apiKey;

    public OpenWeatherDataClient(IConfiguration config, HttpClient httpClient)
    {
        client = httpClient;
        client.BaseAddress = new Uri(config.GetValue<string>("OPENWEATHER_BASE_URL") ?? "");
        apiKey = config.GetValue<string>("OPENWEATHER_API_KEY") ?? "";
    }

    public async Task<decimal> LocationCurrentTemperature(decimal latitude, decimal longitude)
    {
        try
        {
            var response = await client.GetAsync(
                $"?lat={latitude}&lon={longitude}&appid={apiKey}&units=metric"
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiCallException(
                    $"openweather returned bad status: {(ushort)response.StatusCode}"
                );
            }

            var data = await response.Content.ReadFromJsonAsync<OpenWeatherResponse>();
            return data?.Main?.Temp ?? throw new ApiCallException($"failed to decode response");
        }
        catch (HttpRequestException e)
        {
            throw new ApiCallException($"failed to call openweather: {e.Message}.", inner: e);
        }
    }

    public string ProviderName => "OpenWeather";

    public async Task<decimal> CityCurrentTemperature(string cityName)
    {
        try
        {
            var response = await client.GetAsync($"?q={cityName}&appid={apiKey}&units=metric");
            if (!response.IsSuccessStatusCode) throw new ApiCallException("City not found");

            var data = await response.Content.ReadFromJsonAsync<OpenWeatherResponse>();
            return data?.Main?.Temp ?? throw new ApiCallException("Failed to decode");
        }
        catch (HttpRequestException e) { throw new ApiCallException(e.Message); }
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
