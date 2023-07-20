using System;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;

namespace RainBot.UnknownMessageHandler;

public class Handler
{
    private readonly string _accessKey = Environment.GetEnvironmentVariable("SQS_ACCESS_KEY");
    private readonly string _secret = Environment.GetEnvironmentVariable("SQS_SECRET");
    private readonly string _endpointRegion = Environment.GetEnvironmentVariable("SQS_ENDPOINT_REGION");
    private readonly Uri _sendMessageQueue = new(Environment.GetEnvironmentVariable("SEND_MESSAGE_QUEUE"));

    public Handler()
    {
        Guard.IsNotNullOrWhiteSpace(_accessKey);
        Guard.IsNotNullOrWhiteSpace(_secret);
        Guard.IsNotNullOrWhiteSpace(_endpointRegion);
    }

    public async Task<Response> FunctionHandler(Request request)
    {
        Guard.IsNotNull(request);

        var queueMessage = JsonSerializer.Deserialize<QueueInput>(request.Messages[0].Details.Message.Body);

        await SendMessageAsync(queueMessage.Id, MessageTypes.UnknownMessage, queueMessage.LanguageCode, _sendMessageQueue);

        return new Response(200, string.Empty);
    }

    private async Task SendMessageAsync(long id, MessageTypes type, string languageCode, Uri queue)
    {
        using var ymqClient = new YandexMessageQueueClient(_accessKey, _secret, _endpointRegion);

        Console.WriteLine($"Sending a message to {id} ({languageCode}) that {type}");
        await ymqClient.SendMessageAsync(new { ChatId = id, Type = type, LanguageCode = languageCode }, queue, true);
        Console.WriteLine($"Message for {id} ({languageCode}) forwarded to the {queue} queue");
    }
}
