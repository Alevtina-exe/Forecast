using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Forecast.Clients;
using System;
using System.Threading.Tasks;

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
}