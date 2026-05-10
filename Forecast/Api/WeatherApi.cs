using Forecast.Models;
using Forecast.Shared.Responses;
using Forecast.Controllers;
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
            .MapGet("weather/location", WeatherApi.HandleGetCurrentWeather)
            .WithName("GetCurrentWeather")
            .WithTags(["weather"])
            .WithDescription("Возвращает погоду для города (Минск, Лондон...) или координат (lat,lon)");

        groups.MapGet("weather/batch", WeatherApi.HandleGetWeatherBatch)
            .WithName("GetWeatherBatch")
            .WithTags("weather")
            .WithDescription("Получение погоды для списка локаций параллельно");

        groups.MapGet("weather/forecast", WeatherApi.HandleGetForecast)
            .WithName("GetForecast")
            .WithTags("weather")
            .WithDescription("Детальный прогноз (осадки, температура по часам) для одной локации");

        return groups;
    }

    public static async Task<IResult> HandleGetCurrentWeather(
        [FromServices] ICurrentWeatherController controller,
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

    public static async Task<IResult> HandleGetWeatherBatch(
        [FromServices] ICurrentWeatherController controller,
        [FromQuery] string[] locations,
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

    public static async Task<IResult> HandleGetForecast(
    [FromServices] ICurrentWeatherController controller,
    [FromQuery] string location,
    [FromQuery] string provider = "OpenWeather")
    {
        try
        {
            var forecast = await controller.GetWeatherForecast(location, provider);
            return TypedResults.Ok(Success.Create(200, "success", forecast));
        }
        catch (Exception e)
        {
            return TypedResults.BadRequest(Status.Create(400, e.Message));
        }
    }
}
