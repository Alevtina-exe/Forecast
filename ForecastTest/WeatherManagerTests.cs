using Moq;
using Xunit;
using Forecast.Services;

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
}