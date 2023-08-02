using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Mapster;
using RainBot.Core.Models;
using RainBot.Core.Services;
using MyForecast = RainBot.Core.Models.Forecast;

namespace RainBot.YandexWeatherFetcher;

public class WeatherService
{
    private readonly IMessageQueueService _ymqService;
    private readonly HttpClient _http;
    private readonly Uri _forecastHandlerQueue;

    public WeatherService(IMessageQueueService ymqService, HttpClient http, Uri forecastHandlerQueue)
    {
        _ymqService = ymqService;
        _http = http;
        _forecastHandlerQueue = forecastHandlerQueue;
    }
    public async Task FetchAndForwardForecastAsync(string latitude, string longitude)
    {
        var result = await _http.GetFromJsonAsync<InformersResponse>($"https://api.weather.yandex.ru/v2/informers?lat={latitude}&lon={longitude}");



        var forecasts = MapPartsToForecasts(result.Forecast);

        await _ymqService.SendMessageAsync(forecasts, _forecastHandlerQueue);
    }

    private static MyForecast[] MapPartsToForecasts(Forecast forecast)
    {
        var updatedAt = DateTimeOffset.UtcNow;
        var weatherRecords = new MyForecast[2];

        var currentTime = DateTime.UtcNow.AddHours(3);

        for (int i = 0; i < forecast.Parts.Count; i++)
        {
            var forecastPart = forecast.Parts[i];

            var weatherRecord = new MyForecast
            {
                Date = currentTime.Date,
                UpdatedAt = updatedAt,
            };

            forecastPart.Adapt(weatherRecord);

            // If it is between 12 and 18 hours, then we need to add 1 day to Night forecast. Or if it is between 18 and 24 then we need to add 1 day to both records

            if ((currentTime.Hour >= 12 && currentTime.Hour < 18 && weatherRecord.DayTime == DayTime.Night) ||
                (currentTime.Hour >= 18 && currentTime.Hour < 24))
            {
                weatherRecord.Date = weatherRecord.Date.AddDays(1);
            }

            weatherRecords[i] = weatherRecord;
        }

        return weatherRecords;
    }
}
