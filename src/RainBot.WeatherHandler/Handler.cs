using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Runtime;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using Yandex.Cloud.Functions;

namespace RainBot.WeatherHandler;
public class Handler
{
    private readonly string _ydbConnectionString = Environment.GetEnvironmentVariable("YDB_DATABASE");
    private readonly string _accessKey = Environment.GetEnvironmentVariable("SQS_ACCESS_KEY");
    private readonly string _secret = Environment.GetEnvironmentVariable("SQS_SECRET");
    private readonly string _endpointRegion = Environment.GetEnvironmentVariable("SQS_ENDPOINT_REGION");
    private readonly Uri _notificationHandlerQueue = new(Environment.GetEnvironmentVariable("NOTIFICATION_QUEUE"));

    public Handler()
    {
        Guard.IsNotNullOrWhiteSpace(_ydbConnectionString);
        Guard.IsNotNullOrWhiteSpace(_accessKey);
        Guard.IsNotNullOrWhiteSpace(_secret);
        Guard.IsNotNullOrWhiteSpace(_endpointRegion);
    }

    public async Task<Response> FunctionHandler(Request request, Context context)
    {
        Guard.IsNotNull(request);
        Guard.IsNotNull(context);

        var serviceToken = JsonSerializer.Deserialize<ServiceToken>(context.TokenJson);
        Guard.IsNotNullOrWhiteSpace(serviceToken.AccessToken);

        var weatherRecordsFromApi = request.Messages[0].Details.Message.Body;

        using var ydbClient = new YandexDatabaseClient(_ydbConnectionString, serviceToken.AccessToken);
        await ydbClient.Initialize();

        var weatherRecordsFromDatabase = await ydbClient.GetWeatherRecordsByDateAndDayTime(weatherRecordsFromApi);

        await NotifyIfRainAsync(weatherRecordsFromApi, weatherRecordsFromDatabase);

        await SyncWeatherRecordsAsync(ydbClient, weatherRecordsFromApi, weatherRecordsFromDatabase);

        return new Response(200, string.Empty);
    }

    private async Task SyncWeatherRecordsAsync(YandexDatabaseClient ydbClient, IReadOnlyList<WeatherRecord> recordsFromApi, IReadOnlyList<WeatherRecord> recordsFromDatabase)
    {
        for (var i = 0; i < recordsFromDatabase.Count; i++)
        {
            if (recordsFromDatabase[i].IsNotified)
            {
                recordsFromApi.Single(r => r.DayTime == recordsFromDatabase[i].DayTime && r.Date == recordsFromDatabase[i].Date).IsNotified = true;
            }
        }

        await ydbClient.UpsertWeatherRecords(recordsFromApi);
    }

    private async Task NotifyIfRainAsync(IReadOnlyList<WeatherRecord> recordsFromApi, IReadOnlyList<WeatherRecord> recordsFromDatabase)
    {
        var recordsToNotify = new List<WeatherRecord>();

        for (int i = 0; i < recordsFromApi.Count; i++)
        {
            var recordFromDb = recordsFromDatabase.SingleOrDefault(r =>
                r.Date == recordsFromApi[i].Date &&
                r.DayTime == recordsFromApi[i].DayTime &&
                recordsFromApi[i].PrecipitationProbability > 70 &&
                !r.IsNotified);

            if (recordFromDb != null)
            {
                recordsFromApi[i].IsNotified = true;
                recordFromDb.IsNotified = true;
                recordsToNotify.Add(recordsFromApi[i]);
            }
        }

        if (recordsToNotify.Count == 0)
        {
            return;
        }

        using var ymqClient = new YandexMessageQueueClient(_accessKey, _secret, _endpointRegion);

        var message = new
        {
            WeatherRecords = recordsToNotify
        };

        await ymqClient.SendMessageAsync(message, _notificationHandlerQueue, true);
    }
}
