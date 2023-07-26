using System;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Mapster;
using RainBot.Core;
using RainBot.Core.Models;
using RainBot.Core.Models.Functions;
using MyForecast = RainBot.Core.Models.Forecast;

namespace RainBot.YandexWeatherFetcher;

public class Handler
{
    private readonly string _accessKey = Environment.GetEnvironmentVariable("SQS_ACCESS_KEY");
    private readonly string _secret = Environment.GetEnvironmentVariable("SQS_SECRET");
    private readonly string _endpointRegion = Environment.GetEnvironmentVariable("SQS_ENDPOINT_REGION");
    private readonly Uri _forecastHandlerQueue = new Uri(Environment.GetEnvironmentVariable("FORECAST_HANDLER_QUEUE"));

    public Handler()
    {
        SetupMapping();

        Guard.IsNotNullOrWhiteSpace(_accessKey);
        Guard.IsNotNullOrWhiteSpace(_secret);
        Guard.IsNotNullOrWhiteSpace(_endpointRegion);
    }

    public async Task<Response> FunctionHandler(string request)
    {
        var yaWeatherApiKey = Environment.GetEnvironmentVariable("YANDEX_WEATHER_API_KEY");
        Guard.IsNotNullOrWhiteSpace(yaWeatherApiKey);
        var latitude = Environment.GetEnvironmentVariable("LATITUDE");
        Guard.IsNotNullOrWhiteSpace(latitude);
        var longitude = Environment.GetEnvironmentVariable("LONGITUDE");
        Guard.IsNotNullOrWhiteSpace(longitude);

        using var ymqService = new YmqService(_accessKey, _secret, _endpointRegion);
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("X-Yandex-API-Key", yaWeatherApiKey);

        var weatherService = new WeatherService(ymqService, httpClient, _forecastHandlerQueue);
        await weatherService.FetchAndForwardForecastAsync(latitude, longitude);

        return new Response(200, string.Empty);
    }

    private static void SetupMapping()
    {
        TypeAdapterConfig<Part, MyForecast>
            .NewConfig()
              .Map(dest => dest.PrecipitationPeriod, src => src.PrecPeriod)
              .Map(dest => dest.PrecipitationProbability, src => src.PrecProb)
              .Map(dest => dest.Condition, src => src.Condition)
              .Map(dest => dest.DayTime, src => Enum.Parse<DayTime>(src.PartName, true))
              .IgnoreNonMapped(true);
    }
}
