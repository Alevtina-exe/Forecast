using Forecast.Clients;
using Forecast.Controllers;
using Forecast.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
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

    [Fact]
    public async Task GetCurrentWeather_CityName_ReturnsWeather()
    {
        _clientMock.Setup(c => c.CityCurrentTemperature("Minsk")).ReturnsAsync(20m);

        var result = await _controller.GetCurrentWeather("Minsk", "TestProvider");

        Assert.Equal(20m, result);
    }

    [Fact]
    public async Task GetCurrentWeather_Coordinates_ParsesCorrectly()
    {
        _clientMock.Setup(c => c.LocationCurrentTemperature(53.9m, 27.56m)).ReturnsAsync(15m);

        var result = await _controller.GetCurrentWeather("53.9,27.56", "TestProvider");

        Assert.Equal(15m, result);
    }

    [Fact]
    public async Task GetWeatherForecast_ValidCall_ReturnsForecast()
    {
        var expected = new WeatherForecast { City = "Minsk" };
        _clientMock.Setup(c => c.GetWeatherForecastAsync("Minsk")).ReturnsAsync(expected);

        var result = await _controller.GetWeatherForecast("Minsk", "TestProvider");

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetWeatherBatch_MixedResults_ReturnsCollection()
    {
        _clientMock.Setup(c => c.CityCurrentTemperature("Success")).ReturnsAsync(10m);
        _clientMock.Setup(c => c.CityCurrentTemperature("Fail")).ThrowsAsync(new System.Exception("Error"));

        var result = await _controller.GetWeatherBatch(new[] { "Success", "Fail" }, "TestProvider") as OkObjectResult;
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(result.Value).Cast<object>().ToList();

        Assert.Equal(2, list.Count);

        var successResult = list[0].ToString();
        var failResult = list[1].ToString();

        Assert.Contains("Success", successResult);
        Assert.Contains("Error", failResult);
    }

    [Fact]
    public async Task GetClient_UnknownProvider_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<System.ArgumentException>(() =>
            _controller.GetCurrentWeather("Minsk", "Invalid"));
    }
}