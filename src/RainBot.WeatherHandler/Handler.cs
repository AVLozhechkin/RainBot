using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using Yandex.Cloud.Functions;

namespace RainBot.WeatherHandler;
public class Handler
{
    private readonly string _ydbConnectionString = Environment.GetEnvironmentVariable("YDB_DATABASE");

    public Handler()
    {
        Guard.IsNotNullOrWhiteSpace(_ydbConnectionString);
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

        await SyncWeatherRecordsAsync(ydbClient, weatherRecordsFromApi, weatherRecordsFromDatabase);
        

        return new Response(200, string.Empty);
    }

    private async Task SyncWeatherRecordsAsync(YandexDatabaseClient ydbClient, IReadOnlyList<WeatherRecord> recordsReceived, IReadOnlyList<WeatherRecord> recordsFromDatabase)
    {
        for (var i = 0; i < recordsReceived.Count; i++)
        {
            if (recordsFromDatabase[i].IsNotified)
            {
                recordsReceived.Single(r => r.DayTime == recordsFromDatabase[i].DayTime && r.Date == recordsFromDatabase[i].Date).IsNotified = true;
            }
        }

        await ydbClient.UpsertWeatherRecords(recordsReceived);
    }
}
