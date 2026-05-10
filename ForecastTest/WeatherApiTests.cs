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