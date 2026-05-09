using Forecast.Controllers;
using Forecast.Models.Weather;
using Forecast.Shared.Responses;
using Forecast.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Forecast.Api;

public static class WeatherApi
{
    public static RouteGroupBuilder MapCurrentWeatherApi(this RouteGroupBuilder groups)
    {
        groups
            .MapGet("weather/{location}", WeatherApi.HandleGetCurrentWeather)
            .WithName("GetCurrentWeather")
            .WithTags(["weather"])
            .WithDescription("Возвращает погоду для города (Минск, Лондон...) или координат (lat,lon)");

        groups.MapPost("weather/batch", WeatherApi.HandleGetWeatherBatch)
        .WithName("GetWeatherBatch")
        .WithDescription("Получение погоды для списка локаций параллельно");

        return groups;
    }

    private static async Task<IResult> HandleGetCurrentWeather(
        [FromServices] CurrentWeatherController controller,
        string location,
        [FromQuery] string provider = "OpenWeather" 
    )
    {
        try
        {
            var weather = await controller.GetCurrentWeather(location, provider);
            return TypedResults.Ok(Success.Create(200, "success", weather));
        }
        catch (Exception e)
        {
            return TypedResults.BadRequest(Status.Create(400, e.Message));
        }
    }

    private static async Task<IResult> HandleGetWeatherBatch(
    [FromServices] CurrentWeatherController controller,
    [FromBody] string[] locations,
    [FromQuery] string provider = "OpenWeather")
    {
        try
        {
            var results = await controller.GetWeatherBatch(locations, provider);
            return TypedResults.Ok(results);
        }
        catch (Exception e)
        {
            return TypedResults.BadRequest(e.Message);
        }
    }
}
