using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;
using Forecast.Api;
using Forecast.Controllers;
using Forecast.Models;
using Forecast.Shared.Responses;

namespace ForecastTest;

public class WeatherApiTests
{
    /// <summary>
    /// Проверка успешного сценария API: данные от контроллера должны упаковываться в Ok с объектом Success.
    /// </summary>
    [Fact]
    public async Task HandleGetForecast_Success_Test()
    {
        var mockController = new Mock<ICurrentWeatherController>();
        var expected = new WeatherForecast { City = "Minsk" };

        mockController.Setup(c => c.GetWeatherForecast(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(expected);

        var result = await WeatherApi.HandleGetForecast(mockController.Object, "Minsk", "OpenWeather");

        var okResult = Assert.IsType<Ok<Success<WeatherForecast>>>(result);
        Assert.Equal("Minsk", okResult.Value.Data.City);
    }

    /// <summary>
    /// Проверка сценария ошибки API: при исключении в контроллере API должен возвращать BadRequest со статусом ошибки.
    /// </summary>
    [Fact]
    public async Task HandleGetForecast_Error_Test()
    {
        var mockController = new Mock<ICurrentWeatherController>();

        mockController.Setup(c => c.GetWeatherForecast(It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("API Fail"));

        var result = await WeatherApi.HandleGetForecast(mockController.Object, "Minsk", "OpenWeather");

        Assert.IsType<BadRequest<Status>>(result);
    }
}