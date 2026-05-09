namespace Forecast.Models;

public record WeatherResult(
    string City,
    double Temperature,
    string Description,
    string ProviderName
);