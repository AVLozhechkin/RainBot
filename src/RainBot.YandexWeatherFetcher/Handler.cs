﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Mapster;
using RainBot.Core;
using YandexWeatherApi;
using YandexWeatherApi.Extensions;
using Forecast = YandexWeatherApi.Models.InformersModels.Forecast;
using Part = YandexWeatherApi.Models.InformersModels.Part;

namespace RainBot.YandexWeatherFetcher;

public class Handler
{
    private readonly string _yaWeatherApiKey = Environment.GetEnvironmentVariable("YANDEX_WEATHER");
    private readonly string _accessKey = Environment.GetEnvironmentVariable("SQS_ACCESS_KEY");
    private readonly string _secret = Environment.GetEnvironmentVariable("SQS_SECRET");
    private readonly string _endpointRegion = Environment.GetEnvironmentVariable("SQS_ENDPOINT_REGION");
    private readonly Uri _weatherQueue = new Uri(Environment.GetEnvironmentVariable("WEATHER_QUEUE"));

    public Handler()
    {
        SetupMapping();

        Guard.IsNotNullOrWhiteSpace(_yaWeatherApiKey);
        Guard.IsNotNullOrWhiteSpace(_accessKey);
        Guard.IsNotNullOrWhiteSpace(_secret);
        Guard.IsNotNullOrWhiteSpace(_endpointRegion);
    }

    public async Task<Response> FunctionHandler()
    {
        var forecast = await GetForecastAsync(_yaWeatherApiKey);

        var weatherRecords = MapForecastPartsToWeatherRecords(forecast);

        using var ymqClient = new YandexMessageQueueClient(_accessKey, _secret, _endpointRegion);
        await ymqClient.SendMessageAsync(weatherRecords, _weatherQueue, true);

        return new Response(200, string.Empty);
    }

    private static async Task<Forecast> GetForecastAsync(string yaWeatherApiKey)
    {
        var request = YandexWeather.CreateBuilder()
                            .UseApiKey(yaWeatherApiKey)
                            .Build()
                            .Informers()
                            .WithLocale(WeatherLocale.ru_RU)
                            .WithLocality(new WeatherLocality(59.942668, 30.315871));

        var result = await request.Send(new CancellationToken());

        if (result.IsFail)
        {
            Console.WriteLine(result.Error);
        }

        return result.Data.Forecast;
    }

    private static WeatherRecord[] MapForecastPartsToWeatherRecords(Forecast forecast)
    {
        var updatedAt = DateTimeOffset.UtcNow;
        var weatherRecords = new WeatherRecord[2];

        for (int i = 0; i < forecast.Parts.Count; i++)
        {
            var forecastPart = forecast.Parts[i];

            var weatherRecord = new WeatherRecord
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTimeOffset.Parse(forecast.Date),
                UpdatedAt = updatedAt,
            };

            forecastPart.Adapt(weatherRecord);

            weatherRecords[i] = weatherRecord;
        }

        return weatherRecords;
    }

    private static void SetupMapping()
    {
        TypeAdapterConfig<Part, WeatherRecord>
            .NewConfig()
              .Map(dest => dest.PrecipitationPeriod, src => src.PrecPeriod)
              .Map(dest => dest.PrecipitationProbability, src => src.PrecProb)
              .Map(dest => dest.Condition, src => src.Condition)
              .Map(dest => dest.DayTime, src => Enum.Parse<DayTime>(src.PartName, true))
              .IgnoreNonMapped(true);
    }
}
