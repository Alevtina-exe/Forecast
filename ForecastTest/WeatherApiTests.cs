using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;
using Forecast.Api;
using Forecast.Clients;
using Forecast.Controllers;
using Forecast.Models;
using Forecast.Shared.Responses;

namespace ForecastTest;

public class WeatherApiTests
{
    [Fact]
    public async Task HandleGetForecast_Success_StaticTest()
    {
        // Мокаем контроллер (не забудь добавить virtual в CurrentWeatherController!)
        var mockController = new Mock<CurrentWeatherController>(null, null, null);
        var expected = new WeatherForecast { City = "Minsk" };

        mockController.Setup(c => c.GetWeatherForecast(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync(expected);

        // Вызываем статический метод напрямую через имя класса
        var result = await WeatherApi.HandleGetForecast(mockController.Object, "Minsk", "OpenWeather");

        // Проверяем результат
        var okResult = Assert.IsType<Ok<Success<WeatherForecast>>>(result);
        Assert.Equal("Minsk", okResult.Value.Data.City);
    }

    [Fact]
    public async Task HandleGetForecast_Error_StaticTest()
    {
        var mockController = new Mock<CurrentWeatherController>(null, null, null);

        mockController.Setup(c => c.GetWeatherForecast(It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(new Exception("API Fail"));

        // Проверяем обработку исключения в блоке catch статического метода
        var result = await WeatherApi.HandleGetForecast(mockController.Object, "Minsk", "OpenWeather");

        Assert.IsType<BadRequest<Status>>(result);
    }
}