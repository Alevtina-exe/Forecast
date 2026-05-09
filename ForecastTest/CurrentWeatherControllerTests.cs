using Moq;
using Xunit;
using Forecast.Controllers;
using Forecast.Clients;
using Forecast.Models.Weather;

namespace Forecast.Tests;

public class CurrentWeatherControllerTests
{
    [Fact]
    public async Task GetCurrentWeather_ShouldUseCorrectProvider()
    {

        var googleMock = new Mock<IWeatherDataClient>();
        googleMock.Setup(c => c.ProviderName).Returns("Google");
        googleMock.Setup(c => c.CityCurrentTemperature("Минск")).ReturnsAsync(25.0m);

        var openWeatherMock = new Mock<IWeatherDataClient>();
        openWeatherMock.Setup(c => c.ProviderName).Returns("OpenWeather");

        var clients = new List<IWeatherDataClient> { googleMock.Object, openWeatherMock.Object };
        var controller = new CurrentWeatherController(clients);

        var result = await controller.GetCurrentWeather("Минск", "Google");

        Assert.Equal(25.0m, result.Temperature);
        googleMock.Verify(c => c.CityCurrentTemperature("Минск"), Times.Once);
        openWeatherMock.Verify(c => c.CityCurrentTemperature(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetCurrentWeather_ShouldThrowException_WhenProviderNotFound()
    {
        var clients = new List<IWeatherDataClient>(); 
        var controller = new CurrentWeatherController(clients);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            controller.GetCurrentWeather("Минск", "UnknownProvider"));
    }
}