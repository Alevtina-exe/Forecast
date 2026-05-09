using Forecast.Models.Google;
using Forecast.Utils;
using System.Globalization;

namespace Forecast.Clients;

public class GoogleWeatherDataClient : IWeatherDataClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string ProviderName => "Google";

    public GoogleWeatherDataClient(IConfiguration config, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = config.GetValue<string>("GOOGLE_API_KEY") ?? "";
    }

    public async Task<decimal> LocationCurrentTemperature(decimal latitude, decimal longitude)
    {
        try
        {
            var lat = latitude.ToString(CultureInfo.InvariantCulture);
            var lon = longitude.ToString(CultureInfo.InvariantCulture);

            var url = $"https://weather.googleapis.com/v1/currentConditions:lookup?key={_apiKey}&location.latitude={lat}&location.longitude={lon}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ApiCallException($"Google API error: {response.StatusCode}. Details: {error}");
            }

            var data = await response.Content.ReadFromJsonAsync<GoogleWeatherResponse>();
            return data?.Temperature?.Degrees ?? throw new ApiCallException("Failed to parse Google response");
        }
        catch (Exception e) when (e is not ApiCallException)
        {
            throw new ApiCallException($"Google Service unavailable: {e.Message}");
        }
    }

    public Task<decimal> CityCurrentTemperature(string cityName)
    { 
        throw new NotSupportedException("Google Provider требует координаты (lat, lon). Поиск по названию города не поддерживается напрямую.");
    }
}