using Forecast.Clients;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

public class GoogleWeatherDataClientTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly HttpClient _httpClient;

    public GoogleWeatherDataClientTests()
    {
        _configMock = new Mock<IConfiguration>();

        _configMock.Setup(x => x[It.IsAny<string>()]).Returns("fake_api_key");

        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.Value).Returns("fake_api_key");
        _configMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockSection.Object);

        _httpClient = new HttpClient();
    }

    [Fact]
    public async Task GetWeatherForecastAsync_UnsupportedCity_ThrowsArgumentException()
    {
        var client = new GoogleWeatherDataClient(_configMock.Object, _httpClient);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.GetWeatherForecastAsync("UnknownCity"));
    }

    [Fact]
    public async Task GetWeatherForecastAsync_EmptyLocation_ThrowsException()
    {
        var client = new GoogleWeatherDataClient(_configMock.Object, _httpClient);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            client.GetWeatherForecastAsync(""));
    }

    [Fact]
    public void ProviderName_IsGoogle()
    {
        var client = new GoogleWeatherDataClient(_configMock.Object, _httpClient);
        Assert.Equal("Google", client.ProviderName);
    }

    [Fact]
    public async Task GoogleWeather_LocationCurrentTemperature_Success_IncreasesCoverage()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c.GetSection("GOOGLE_API_KEY").Value).Returns("fake_google_key");

        var jsonResponse = "{ \"temperature\": { \"degrees\": 15.7 } }";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new GoogleWeatherDataClient(mockConfig.Object, httpClient);

        // Act
        var temp = await client.LocationCurrentTemperature(53.9m, 27.5m);

        // Assert
        Assert.Equal(15.7m, temp);
    }

    [Fact]
    public async Task Google_LocationCurrentTemperature_ReturnsValue()
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c.GetSection("GOOGLE_API_KEY").Value).Returns("key");

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ \"temperature\": { \"degrees\": 20.0 } }", System.Text.Encoding.UTF8, "application/json")
            });

        var client = new GoogleWeatherDataClient(mockConfig.Object, new HttpClient(handlerMock.Object));

        var result = await client.LocationCurrentTemperature(53.9m, 27.5m);

        Assert.Equal(20.0m, result);
    }
}