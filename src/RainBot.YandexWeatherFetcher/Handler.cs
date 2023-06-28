// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using RainBot.Core;
using YandexWeatherApi;
using YandexWeatherApi.Extensions;
using YandexWeatherApi.Models.InformersModels;
using YandexWeatherApi.Result;
using Forecast = YandexWeatherApi.Models.InformersModels.Forecast;
using Part = YandexWeatherApi.Models.InformersModels.Part;

namespace RainBot.YandexWeatherFetcher;

public class Handler
{
    public async Task<Response> FunctionHandler()
    {
        SetupMapping();

        var yaWeatherApiKey = Environment.GetEnvironmentVariable("YANDEX_WEATHER");
        var accessKey = Environment.GetEnvironmentVariable("SQS_ACCESS_KEY");
        var secret = Environment.GetEnvironmentVariable("SQS_SECRET");
        var endpointRegion = Environment.GetEnvironmentVariable("SQS_ENDPOINT_REGION");
        var isWeatherQueueProvided = Uri.TryCreate(Environment.GetEnvironmentVariable("WEATHER_QUEUE"), UriKind.Absolute, out Uri weatherQueue);

        if (string.IsNullOrWhiteSpace(yaWeatherApiKey) ||
            string.IsNullOrWhiteSpace(accessKey) ||
            string.IsNullOrWhiteSpace(secret) ||
            string.IsNullOrWhiteSpace(endpointRegion) ||
            !isWeatherQueueProvided)
        {
            Console.WriteLine("The function does not work because not all environment variables are specified");
            return new Response(500, string.Empty);
        }

        var forecast = await GetForecastAsync(yaWeatherApiKey);

        var weatherRecords = TransformForecastToWeatherRecords(forecast);

        using var ymqClient = new YandexMessageQueueClient(accessKey, secret, endpointRegion);
        await ymqClient.SendMessageAsync(weatherRecords, weatherQueue, true);

        return new Response(200, string.Empty);
    }

    private static async Task<Forecast> GetForecastAsync(string yaWeatherApiKey)
    {
        IYandexWeatherInformersRequest request = YandexWeather.CreateBuilder()
                            .UseApiKey(yaWeatherApiKey)
                            .Build()
                            .Informers()
                            .WithLocale(WeatherLocale.ru_RU)
                            .WithLocality(new WeatherLocality(59.942668, 30.315871));

        Result<InformersResponse> result = await request.Send(new CancellationToken());

        if (result.IsFail)
        {
            Console.WriteLine(result.Error);
        }

        return result.Data.Forecast;
    }

    private static WeatherRecord[] TransformForecastToWeatherRecords(Forecast forecast)
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
