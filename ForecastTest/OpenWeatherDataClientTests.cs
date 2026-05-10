using Forecast.Clients;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

public class OpenWeatherDataClientTests
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public OpenWeatherDataClientTests()
    {
        _httpClient = new HttpClient();
        var myConfiguration = new Dictionary<string, string>
        {
            {"OpenWeatherApiKey", "fake_api_key"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }

    [Fact]
    public void ProviderName_IsOpenWeather()
    {
        var client = new OpenWeatherDataClient(_configuration, _httpClient);
        Assert.Equal("OpenWeather", client.ProviderName);
    }

    [Theory]
    [InlineData("invalid_coords")]
    [InlineData("123,abc")]
    public async Task GetForecastAsync_InvalidFormat_ThrowsException(string input)
    {
        var client = new OpenWeatherDataClient(_configuration, _httpClient);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            client.GetWeatherForecastAsync(input));
    }

}