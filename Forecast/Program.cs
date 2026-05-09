using Forecast.Api;
using Forecast.Clients;
using Forecast.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Loading configuration as environment variables from the .env file.
DotNetEnv.Env.Load();
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "WeatherExampleAPI";
    config.Title = "Weather Example API";
    config.Version = "v1";
});
builder.Services.AddHttpClient<IWeatherDataClient, OpenWeatherDataClient>();
builder.Services.AddHttpClient<IWeatherDataClient, GoogleWeatherDataClient>();
builder.Services.AddScoped<CurrentWeatherController>();
builder.Services.AddSingleton<IWeatherDataClient, OpenWeatherDataClient>();
builder.Services.AddSingleton<CurrentWeatherController>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
    app.UseDeveloperExceptionPage();
}

app.MapGroup("/api/v1").MapCurrentWeatherApi();

app.Run();
