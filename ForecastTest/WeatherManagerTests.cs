using Forecast.Clients;
using Forecast.Models;
using Forecast.Services;
using Moq;
using Xunit;

public class WeatherManagerTests
{
    [Fact]
    public async Task GetWeather_ShouldUseGoogleProvider_WhenRequested()
    {
        var googleMock = new Mock<IWeatherProvider>();
        googleMock.Setup(p => p.Name).Returns("Google");

        var providers = new List<IWeatherProvider> { googleMock.Object };

        var manager = new WeatherManager(providers);


        await manager.GetForecastAsync("Minsk", "Google");

        googleMock.Verify(p => p.GetWeatherByCityAsync("Minsk"), Times.Once);
    }
    [Fact]
    public async Task GetWeatherForMultipleCities_ShouldReturnResultsForEachCity()
    {

        var googleMock = new Mock<IWeatherProvider>();
        googleMock.Setup(p => p.Name).Returns("Google");

        googleMock.Setup(p => p.GetWeatherByCityAsync(It.IsAny<string>()))
                  .ReturnsAsync((string city) => new WeatherResult(city, 20.0, "Sunny", "Google"));

        var manager = new WeatherManager(new List<IWeatherProvider> { googleMock.Object });
        var cities = new List<string> { "Минск", "Лондон" };


        var results = await manager.GetWeatherForMultipleCitiesAsync(cities, "Google");

        Assert.Equal(2, results.Count());
        googleMock.Verify(p => p.GetWeatherAsync(It.IsAny<double>(), It.IsAny<double>()), Times.Exactly(2));
    }
    [Fact]
    public async Task ShouldReturnCoordinatesForKnownCity()
    {
        var googleMock = new Mock<IWeatherDataClient>();
        googleMock.Setup(c => c.ProviderName).Returns("Google");

        var clients = new List<IWeatherDataClient> { googleMock.Object };

    }
}