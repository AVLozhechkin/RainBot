using System;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Newtonsoft.Json;
using RainBot.Core;
using Telegram.Bot.Types;

namespace RainBot.TelegramHandler;

public class Handler
{
    private readonly string _accessKey = Environment.GetEnvironmentVariable("SQS_ACCESS_KEY");
    private readonly string _secret = Environment.GetEnvironmentVariable("SQS_SECRET");
    private readonly string _endpointRegion = Environment.GetEnvironmentVariable("SQS_ENDPOINT_REGION");
    private readonly Uri _startQueue = new(Environment.GetEnvironmentVariable("START_QUEUE"));
    private readonly Uri _stopQueue = new(Environment.GetEnvironmentVariable("STOP_QUEUE"));
    private readonly Uri _unknownQueue = new(Environment.GetEnvironmentVariable("UNKNOWN_QUEUE"));

    public async Task<Response> FunctionHandler(Request request)
    {
        Guard.IsNotNull(request);
        Guard.IsNotNullOrWhiteSpace(_accessKey);
        Guard.IsNotNullOrWhiteSpace(_secret);
        Guard.IsNotNullOrWhiteSpace(_endpointRegion);

        var update = JsonConvert.DeserializeObject<Update>(request.body);

        if (update.Message is null)
        {
            return new Response(200, string.Empty);
        }

        var queueMessage = new { update.Message.From.Id, update.Message.From.LanguageCode };

        var destQueue = update.Message.Text switch
        {
            "/start" => _startQueue,
            "/stop" => _stopQueue,
            _ => _unknownQueue
        };

        await SendMessageToTheQueue(destQueue, update.Message, queueMessage);
        return new Response(200, string.Empty);
    }

    private async Task SendMessageToTheQueue(Uri queue, Message receivedMessage, object message)
    {
        using var client = new YandexMessageQueueClient(_accessKey, _secret, _endpointRegion);

        Console.WriteLine($"Received a \"{receivedMessage.Text}\" message from {receivedMessage.From.Id} ({receivedMessage.From.LanguageCode})");
        await client.SendMessageAsync(message, queue, true);
        Console.WriteLine($"Message from {receivedMessage.From.Id} forwarded to the {queue} queue");
    }
}
