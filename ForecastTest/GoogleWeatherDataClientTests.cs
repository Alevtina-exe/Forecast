using Forecast.Clients;
using Forecast.Utils;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Проверка выброса исключения при запросе прогноза для города, отсутствующего в реестре.
    /// </summary>
    [Fact]
    public async Task GetWeatherForecastAsync_UnsupportedCity_ThrowsArgumentException()
    {
        var client = new GoogleWeatherDataClient(_configMock.Object, _httpClient);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.GetWeatherForecastAsync("UnknownCity"));
    }

    /// <summary>
    /// Проверка обработки пустой строки вместо локации.
    /// </summary>
    [Fact]
    public async Task GetWeatherForecastAsync_EmptyLocation_ThrowsException()
    {
        var client = new GoogleWeatherDataClient(_configMock.Object, _httpClient);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            client.GetWeatherForecastAsync(""));
    }

    /// <summary>
    /// Проверка корректности возвращаемого имени провайдера.
    /// </summary>
    [Fact]
    public void ProviderName_IsGoogle()
    {
        var client = new GoogleWeatherDataClient(_configMock.Object, _httpClient);
        Assert.Equal("Google", client.ProviderName);
    }

    /// <summary>
    /// Тестирование успешного получения текущей температуры по координатам и маппинга JSON.
    /// </summary>
    [Fact]
    public async Task GoogleWeather_LocationCurrentTemperature_Success_IncreasesCoverage()
    {
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

        var temp = await client.LocationCurrentTemperature(53.9m, 27.5m);

        Assert.Equal(15.7m, temp);
    }

    /// <summary>
    /// Проверка выброса ArgumentException при поиске температуры в неподдерживаемом городе.
    /// </summary>
    [Fact]
    public async Task Google_UnknownCity_ThrowsArgumentException()
    {
        var mockConfig = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.Value).Returns("fake_key");
        mockConfig.Setup(c => c.GetSection("GOOGLE_API_KEY")).Returns(mockSection.Object);

        var client = new GoogleWeatherDataClient(mockConfig.Object, new HttpClient());

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            client.CityCurrentTemperature("UnknownCity")
        );

        Assert.Contains("Минск", exception.Message);
        Assert.Contains("Лондон", exception.Message);
    }

    /// <summary>
    /// Тестирование успешного получения почасового прогноза и корректности парсинга вложенных полей Google API.
    /// </summary>
    [Fact]
    public async Task Google_GetWeatherForecastAsync_Success_Final()
    {
        var mockConfig = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.Value).Returns("fake_key");
        mockConfig.Setup(c => c.GetSection("GOOGLE_API_KEY")).Returns(mockSection.Object);

        var jsonResponse = @"
        {
            ""forecastHours"": [
                {
                    ""interval"": { ""startTime"": ""2026-05-10T12:00:00Z"" },
                    ""temperature"": { ""degrees"": 15.5 },
                    ""weatherCondition"": { ""type"": ""CLEAR"" },
                    ""precipitation"": { ""probability"": { ""percent"": 5 } }
                }
            ]
        }";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new GoogleWeatherDataClient(mockConfig.Object, httpClient);

        var result = await client.GetWeatherForecastAsync("Минск");

        Assert.NotNull(result);
        Assert.Equal("Минск", result.City);
        Assert.Single(result.Items);
        Assert.Equal(15.5m, result.Items[0].Temperature);
        Assert.Equal("CLEAR", result.Items[0].Condition);
    }

    /// <summary>
    /// Проверка обработки 500 ошибки (Internal Server Error) от Google и выброса ApiCallException.
    /// </summary>
    [Fact]
    public async Task Google_LocationCurrentTemperature_ThrowsApiCallException_OnInternalError()
    {
        var mockConfig = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.Value).Returns("fake_key");
        mockConfig.Setup(c => c.GetSection("GOOGLE_API_KEY")).Returns(mockSection.Object);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var client = new GoogleWeatherDataClient(mockConfig.Object, new HttpClient(handlerMock.Object));

        await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationCurrentTemperature(53.9m, 27.5m)
        );
    }
}