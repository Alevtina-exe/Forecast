using Forecast.Clients;
using Forecast.Controllers;
using Forecast.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class CurrentWeatherControllerTests
{
    private readonly Mock<IWeatherDataClient> _clientMock;
    private readonly CurrentWeatherController _controller;

    public CurrentWeatherControllerTests()
    {
        _clientMock = new Mock<IWeatherDataClient>();
        _clientMock.Setup(c => c.ProviderName).Returns("TestProvider");
        _controller = new CurrentWeatherController(new[] { _clientMock.Object });
    }

    /// <summary>
    /// Проверка базового получения температуры по названию города через контроллер.
    /// </summary>
    [Fact]
    public async Task GetCurrentWeather_CityName_ReturnsWeather()
    {
        _clientMock.Setup(c => c.CityCurrentTemperature("Minsk")).ReturnsAsync(20m);

        var result = await _controller.GetCurrentWeather("Minsk", "TestProvider");

        Assert.Equal(20m, result);
    }

    /// <summary>
    /// Тестирование логики парсинга строки с координатами (latitude, longitude) внутри контроллера.
    /// </summary>
    [Fact]
    public async Task GetCurrentWeather_Coordinates_ParsesCorrectly()
    {
        _clientMock.Setup(c => c.LocationCurrentTemperature(53.9m, 27.56m)).ReturnsAsync(15m);

        var result = await _controller.GetCurrentWeather("53.9,27.56", "TestProvider");

        Assert.Equal(15m, result);
    }

    /// <summary>
    /// Проверка корректного проброса запроса на прогноз погоды к выбранному провайдеру.
    /// </summary>
    [Fact]
    public async Task GetWeatherForecast_ValidCall_ReturnsForecast()
    {
        var expected = new WeatherForecast { City = "Minsk" };
        _clientMock.Setup(c => c.GetWeatherForecastAsync("Minsk")).ReturnsAsync(expected);

        var result = await _controller.GetWeatherForecast("Minsk", "TestProvider");

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Тестирование пакетного запроса (Batch). Проверяет, что контроллер возвращает результаты 
    /// для всех локаций, даже если одна из них завершилась ошибкой.
    /// </summary>
    [Fact]
    public async Task GetWeatherBatch_MixedResults_ReturnsCollection()
    {
        var mockClient = new Mock<IWeatherDataClient>();
        mockClient.Setup(c => c.ProviderName).Returns("OpenWeather");

        mockClient.Setup(c => c.CityCurrentTemperature("Minsk"))
                  .ReturnsAsync(20m);

        mockClient.Setup(c => c.CityCurrentTemperature("ErrorCity"))
                  .ThrowsAsync(new Exception("Not Found"));

        var clients = new List<IWeatherDataClient> { mockClient.Object };
        var controller = new CurrentWeatherController(clients);
        var locations = new[] { "Minsk", "ErrorCity" };

        var result = await controller.GetWeatherBatch(locations, "OpenWeather");

        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
    }

    /// <summary>
    /// Проверка защитной логики контроллера: выброс исключения при запросе несуществующего провайдера.
    /// </summary>
    [Fact]
    public async Task GetClient_UnknownProvider_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<System.ArgumentException>(() =>
            _controller.GetCurrentWeather("Minsk", "Invalid"));
    }
}