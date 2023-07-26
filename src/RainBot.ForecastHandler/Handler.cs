using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using RainBot.Core.Models;
using RainBot.Core.Models.Functions;
using Yandex.Cloud.Functions;

namespace RainBot.ForecastHandler;
public class Handler
{
    private readonly string _databasePath = Environment.GetEnvironmentVariable("YDB_DATABASE");
    private readonly string _accessKey = Environment.GetEnvironmentVariable("SQS_ACCESS_KEY");
    private readonly string _secret = Environment.GetEnvironmentVariable("SQS_SECRET");
    private readonly string _endpointRegion = Environment.GetEnvironmentVariable("SQS_ENDPOINT_REGION");
    private readonly Uri _sendMessageQueue = new(Environment.GetEnvironmentVariable("SEND_MESSAGE_QUEUE"));

    public Handler()
    {
        Guard.IsNotNullOrWhiteSpace(_databasePath);
        Guard.IsNotNullOrWhiteSpace(_accessKey);
        Guard.IsNotNullOrWhiteSpace(_secret);
        Guard.IsNotNullOrWhiteSpace(_endpointRegion);
    }

    public async Task<Response> FunctionHandler(QueueRequest request, Context context)
    {
        Guard.IsNotNull(request);
        Guard.IsNotNull(context);

        var serviceToken = JsonSerializer.Deserialize<ServiceToken>(context.TokenJson);
        Guard.IsNotNullOrWhiteSpace(serviceToken.AccessToken);

        var weatherRecordsFromApi = JsonSerializer.Deserialize<IReadOnlyList<Forecast>>(request.Messages[0].Details.Message.Body);

        using var driver = DriverExtensions.Build(_databasePath, serviceToken.AccessToken);
        await driver.Initialize();

        var forecastRepository = new ForecastRepository(driver);

        var forecastService = new ForecastService(forecastRepository);

        var rainyForecasts = await forecastService.SyncForecasts(weatherRecordsFromApi);

        if (rainyForecasts.Count > 0)
        {
            var ymqService = new YmqService(_accessKey, _secret, _endpointRegion);
            var subscriptionRepository = new SubscriptionRepository(driver);

            var notificationService = new NotificationService(subscriptionRepository, ymqService, _sendMessageQueue);

            await notificationService.SendNotifications(rainyForecasts);
        }

        return new Response(200, string.Empty);
    }
}
