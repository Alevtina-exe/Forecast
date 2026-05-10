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

    /// <summary>
    /// Проверка корректности имени провайдера.
    /// </summary>
    [Fact]
    public void ProviderName_IsOpenWeather()
    {
        var client = new OpenWeatherDataClient(_configuration, _httpClient);
        Assert.Equal("OpenWeather", client.ProviderName);
    }

    /// <summary>
    /// Тестирование обработки неверного формата координат.
    /// </summary>
    [Theory]
    [InlineData("invalid_coords")]
    [InlineData("123,abc")]
    public async Task GetForecastAsync_InvalidFormat_ThrowsException(string input)
    {
        var client = new OpenWeatherDataClient(_configuration, _httpClient);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            client.GetWeatherForecastAsync(input));
    }

    /// <summary>
    /// Тестирование успешного получения прогноза и десериализации базовых полей.
    /// </summary>
    [Fact]
    public async Task OpenWeather_GetWeatherForecastAsync_Success_IncreasesCoverage()
    {
        var mockConfig = new Mock<IConfiguration>();
        var mockApiKeySection = new Mock<IConfigurationSection>();
        mockApiKeySection.Setup(s => s.Value).Returns("fake_key");

        var mockBaseUrlSection = new Mock<IConfigurationSection>();
        mockBaseUrlSection.Setup(s => s.Value).Returns("https://api.openweathermap.org/data/2.5/forecast");
        mockConfig.Setup(c => c.GetSection("OPENWEATHER_API_KEY")).Returns(mockApiKeySection.Object);
        mockConfig.Setup(c => c.GetSection("OPENWEATHER_BASE_URL")).Returns(mockBaseUrlSection.Object);

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

        var result = await client.GetWeatherForecastAsync("Minsk");

        Assert.NotNull(result);
        Assert.Equal("Minsk", result.City);
    }

    /// <summary>
    /// Проверка расчета вероятности осадков и маппинга данных прогноза.
    /// </summary>
    [Fact]
    public async Task OpenWeather_GetWeatherForecastAsync_Success()
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c.GetSection("OPENWEATHER_API_KEY").Value).Returns("fake_key");
        mockConfig.Setup(c => c.GetSection("OPENWEATHER_BASE_URL").Value).Returns("https://api.openweathermap.org/data/2.5/");

        var jsonResponse = @"
    {
        ""city"": { ""name"": ""Minsk"" },
        ""list"": [
            {
                ""dt"": 1715335200,
                ""main"": { ""temp"": 20.5 },
                ""weather"": [{ ""main"": ""Clouds"" }],
                ""pop"": 0.45
            }
        ]
    }";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            });

        var client = new OpenWeatherDataClient(mockConfig.Object, new HttpClient(handlerMock.Object));

        var result = await client.GetWeatherForecastAsync("Minsk");

        Assert.Equal("Minsk", result.City);
        Assert.Equal(20.5m, result.Items[0].Temperature);
        Assert.Equal(45, result.Items[0].PrecipitationProbability);
        Assert.Equal("Clouds", result.Items[0].Condition);
    }

    /// <summary>
    /// Тестирование обработки ошибки 404 (город не найден) от внешнего API.
    /// </summary>
    [Fact]
    public async Task OpenWeather_CityCurrentTemperature_NotFound_ThrowsApiCallException()
    {
        var mockConfig = new Mock<IConfiguration>();

        var mockApiKeySection = new Mock<IConfigurationSection>();
        mockApiKeySection.Setup(s => s.Value).Returns("fake_key");
        mockConfig.Setup(c => c.GetSection("OPENWEATHER_API_KEY")).Returns(mockApiKeySection.Object);

        var mockUrlSection = new Mock<IConfigurationSection>();
        mockUrlSection.Setup(s => s.Value).Returns("https://api.openweathermap.org/data/2.5/");
        mockConfig.Setup(c => c.GetSection("OPENWEATHER_BASE_URL")).Returns(mockUrlSection.Object);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new OpenWeatherDataClient(mockConfig.Object, httpClient);

        var exception = await Assert.ThrowsAsync<ApiCallException>(() =>
            client.CityCurrentTemperature("NonExistentCity")
        );

        Assert.Contains("not found", exception.Message.ToLower());
    }

    /// <summary>
    /// Проверка обработки критических сбоев сети и проброса ApiCallException.
    /// </summary>
    [Fact]
    public async Task OpenWeather_NetworkFailure_ThrowsApiCallException()
    {
        var mockConfig = new Mock<IConfiguration>();

        var mockApiKeySection = new Mock<IConfigurationSection>();
        mockApiKeySection.Setup(s => s.Value).Returns("fake_key");
        mockConfig.Setup(c => c.GetSection("OPENWEATHER_API_KEY")).Returns(mockApiKeySection.Object);

        var mockUrlSection = new Mock<IConfigurationSection>();
        mockUrlSection.Setup(s => s.Value).Returns("https://api.openweathermap.org/");
        mockConfig.Setup(c => c.GetSection("OPENWEATHER_BASE_URL")).Returns(mockUrlSection.Object);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("No internet connection"));

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new OpenWeatherDataClient(mockConfig.Object, httpClient);

        var exception = await Assert.ThrowsAsync<ApiCallException>(() =>
            client.LocationCurrentTemperature(53.9m, 27.5m)
        );

        Assert.Contains("failed to call openweather", exception.Message);
    }
}