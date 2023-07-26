using System;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using RainBot.Core.Dto;
using RainBot.Core.Models;
using RainBot.Core.Models.Functions;
using Yandex.Cloud.Functions;

namespace RainBot.SubscriptionHandler;

public class Handler : YcFunction<QueueRequest, Task<Response>>
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

        var subscriptionRequest = JsonSerializer.Deserialize<SubscriptionRequest>(request.Messages[0].Details.Message.Body);

        using var driver = DriverExtensions.Build(_databasePath, serviceToken.AccessToken);
        await driver.Initialize();

        using var ymqService = new YmqService(_accessKey, _secret, _endpointRegion);

        var subscriptionRepository = new SubscriptionRepository(driver);

        var subscriptionService = new SubscriptionService(subscriptionRepository, ymqService, _sendMessageQueue);

        switch (subscriptionRequest.Operation)
        {
            case SubscriptionRequest.SubscriptionOperation.Add:
                await subscriptionService.AddSubscription(subscriptionRequest.ChatId, subscriptionRequest.LanguageCode);
                break;
            case SubscriptionRequest.SubscriptionOperation.Remove:
                await subscriptionService.RemoveSubscription(subscriptionRequest.ChatId, subscriptionRequest.LanguageCode);
                break;
            default:
                throw new ArgumentException($"Unknown subscription operation: {subscriptionRequest.Operation}");
        }

        return new Response(200, string.Empty);
    }


}
