using Forecast.Clients;
using Forecast.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForecastTest
{
    public class WeatherTests
    {
        [Fact]
        public async Task ShouldThrowWhenProviderInvalid()
        {

            var mockClient = new Mock<IWeatherDataClient>();
            mockClient.Setup(c => c.ProviderName).Returns("Google");
            var controller = new CurrentWeatherController(new[] { mockClient.Object });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                controller.GetCurrentWeather("Минск", "Yandex"));
        }
    }
}
