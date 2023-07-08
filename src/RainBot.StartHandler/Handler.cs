using System;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using Yandex.Cloud.Functions;

namespace RainBot.StartHandler;

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

        var insertResult = await TryInsertSubscriptionAsync(serviceToken.AccessToken, queueMessage.Id, queueMessage.LanguageCode);

        var messageToSend = insertResult switch
        {
            YdbQueryResult.Ok => MessageTypes.SubscriptionAdded,
            YdbQueryResult.AlreadyExist => MessageTypes.SubscriptionAlreadyExist,
            _ => MessageTypes.SomethingWentWrong,
        };
        await SendMessageAsync(queueMessage.Id, messageToSend, queueMessage.LanguageCode, _sendMessageQueue);

        return new Response(200, string.Empty);
    }

    private async Task<YdbQueryResult> TryInsertSubscriptionAsync(string accessToken, ulong chatId, string languageCode)
    {
        using var ydbService = new YandexDatabaseClient(_ydbConnectionString, accessToken);
        await ydbService.Initialize();

        Console.WriteLine($"Inserting a subscription for chatId {chatId} ({languageCode})");
        var insertResult = await ydbService.InsertSubscriptionAsync(chatId, languageCode);
        Console.WriteLine($"Insert result for {chatId} is {insertResult}");

        return insertResult;
    }

    private async Task SendMessageAsync(long id, MessageTypes type, string languageCode, Uri queue)
    {
        var sendMessageDto = new SendMessageDto
        {
            ChatId = id,
            LanguageCode = languageCode,
            Text = MessageStrings.RussianMessages.Value[type]
        };

        using var ymqClient = new YandexMessageQueueClient(_accessKey, _secret, _endpointRegion);

        Console.WriteLine($"Sending a message to {id} ({languageCode}) that {type}");
        await ymqClient.SendMessageAsync(sendMessageDto, queue, true);
        Console.WriteLine($"Message for {id} ({languageCode}) forwarded to the {queue} queue");
    }
}
