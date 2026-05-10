using Forecast.Clients;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
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

    [Fact]
    public async Task OpenWeather_GetWeatherForecastAsync_Success_IncreasesCoverage()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c.GetSection("OPENWEATHER_API_KEY").Value).Returns("fake_key");

        var jsonResponse = "{ \"city\": { \"name\": \"Minsk\" }, \"list\": [ { \"dt\": 1715335200, \"main\": { \"temp\": 20.5 }, \"weather\": [{ \"main\": \"Clouds\" }], \"pop\": 0.1 } ] }";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new OpenWeatherDataClient(mockConfig.Object, httpClient);

        // Act
        var result = await client.GetWeatherForecastAsync("Minsk");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Minsk", result.City);
        Assert.Single(result.Items);
        Assert.Equal(20.5m, result.Items[0].Temperature);
    }

}