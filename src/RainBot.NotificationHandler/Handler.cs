using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using Yandex.Cloud.Functions;

namespace RainBot.NotificationHandler;

public class Handler : YcFunction<Request, Task<Response>>
{
    private readonly string _ydbConnectionString = Environment.GetEnvironmentVariable("YDB_DATABASE");
    private readonly string _accessKey = Environment.GetEnvironmentVariable("SQS_ACCESS_KEY");
    private readonly string _secret = Environment.GetEnvironmentVariable("SQS_SECRET");
    private readonly string _endpointRegion = Environment.GetEnvironmentVariable("SQS_ENDPOINT_REGION");
    private readonly Uri _sendMessageQueue = new(Environment.GetEnvironmentVariable("SEND_MESSAGE_QUEUE"));

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

        var weatherRecords = request.Messages[0].Details.Message.Body.WeatherRecords;

        var subscriptions = (await GetSubscriptionsAsync(serviceToken.AccessToken)).ToLookup(s => s.LanguageCode == "en");

        using var ymqClient = new YandexMessageQueueClient(_accessKey, _secret, _endpointRegion);

        var englishSubscribers = subscriptions[true];

        if (englishSubscribers.Any())
        {
            var message = MessageService.BuildNotificationMessage(weatherRecords, "en");

            await SendNotificationsAsync(ymqClient, englishSubscribers, message);
        }

        var russianSubscribers = subscriptions[false];

        if (russianSubscribers.Any())
        {
            var message = MessageService.BuildNotificationMessage(weatherRecords);

            await SendNotificationsAsync(ymqClient, russianSubscribers, message);
        }

        return new Response(200, string.Empty);
    }

    private async Task<IReadOnlyList<Subscription>> GetSubscriptionsAsync(string accessToken)
    {
        using var ydbClient = new YandexDatabaseClient(_ydbConnectionString, accessToken);
        await ydbClient.Initialize();

        var subscriptions = await ydbClient.GetSubscriptionsAsync();

        Guard.HasSizeNotEqualTo(subscriptions, 0);

        return subscriptions;
    }

    private async Task SendNotificationsAsync(YandexMessageQueueClient ymqClient, IEnumerable<Subscription> subscriptions, string message)
    {
        foreach (var subscription in subscriptions)
        {
            var sendMessageDto = new SendMessageDto
            {
                ChatId = subscription.ChatId,
                LanguageCode = subscription.LanguageCode,
                Text = message
            };

            await ymqClient.SendMessageAsync(sendMessageDto, _sendMessageQueue, true);
        }
    }

    
}
