using System;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using Yandex.Cloud.Functions;

namespace RainBot.StopHandler;

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

        var queueMessage = request.Messages[0].Details.Message.Body;

        var removeResult = await RemoveSubscriptionAsync(serviceToken.AccessToken, queueMessage.Id, queueMessage.LanguageCode);

        if (removeResult == YdbQueryResult.Ok)
        {
            await SendMessageAsync(queueMessage.Id, MessageTypes.SubscriptionRemoved, queueMessage.LanguageCode, _sendMessageQueue);
        }
        else
        {
            await SendMessageAsync(queueMessage.Id, MessageTypes.SomethingWentWrong, queueMessage.LanguageCode, _sendMessageQueue);
        }

        return new Response(200, string.Empty);
    }

    private async Task<YdbQueryResult> RemoveSubscriptionAsync(string accessToken, ulong chatId, string languageCode)
    {
        using var ydbClient = new YandexDatabaseClient(_ydbConnectionString, accessToken);
        await ydbClient.Initialize();

        Console.WriteLine($"Removing a subscription for chatId {chatId} ({languageCode})");
        var removeResult = await ydbClient.RemoveSubscriptionAsync(chatId);
        Console.WriteLine($"Remove result for {chatId} is {removeResult}");

        return removeResult;
    }

    private async Task SendMessageAsync(ulong id, MessageTypes type, string languageCode, Uri queue)
    {
        using var ymqClient = new YandexMessageQueueClient(_accessKey, _secret, _endpointRegion);

        Console.WriteLine($"Sending a message to {id} ({languageCode}) that {type}");
        await ymqClient.SendMessageAsync(new { ChatId = id, Type = type, LanguageCode = languageCode }, queue, true);
        Console.WriteLine($"Message for {id} ({languageCode}) forwarded to the {queue} queue");
    }
}
