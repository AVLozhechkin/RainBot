using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Newtonsoft.Json;
using RainBot.Core;
using System.Linq;
using RainBot.TelegramHandler.Strategies;
using Telegram.Bot.Types;
using RainBot.Core.Models.Functions;

namespace RainBot.TelegramHandler;

public class Handler
{
    private readonly string _accessKey = Environment.GetEnvironmentVariable("SQS_ACCESS_KEY");
    private readonly string _secret = Environment.GetEnvironmentVariable("SQS_SECRET");
    private readonly string _endpointRegion = Environment.GetEnvironmentVariable("SQS_ENDPOINT_REGION");
    private readonly Uri _subscriptionHandlerQueue = new(Environment.GetEnvironmentVariable("SUBSCRIPTION_HANDLER_QUEUE"));
    private readonly Uri _sendMessageQueue = new(Environment.GetEnvironmentVariable("SEND_MESSAGE_QUEUE"));

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

        var ymqService = new YmqService(_accessKey, _secret, _endpointRegion);

        var strategies = new List<IMessageProcessStrategy>
        {
            new StartStrategy(ymqService, _subscriptionHandlerQueue),
            new StopStrategy(ymqService, _subscriptionHandlerQueue)
        };

        var strategy = strategies.SingleOrDefault(s => s.CanBeExecuted(update.Message.Text), new DefaultStrategy(ymqService, _sendMessageQueue));

        await strategy!.ExecuteAsync(update.Message);

        return new Response(200, string.Empty);
    }
}
