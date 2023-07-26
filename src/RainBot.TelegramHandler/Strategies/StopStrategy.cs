using System;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using RainBot.Core.Dto;
using RainBot.Core.Services;
using Telegram.Bot.Types;

namespace RainBot.TelegramHandler.Strategies;

public class StopStrategy : IMessageProcessStrategy
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly Uri _subscriptionHandlerQueue;

    public StopStrategy(IMessageQueueService messageQueueService, Uri subscriptionHandlerQueue)
    {
        _messageQueueService = messageQueueService;
        _subscriptionHandlerQueue = subscriptionHandlerQueue;
    }
    public bool CanBeExecuted(string message)
    {
        Guard.IsNotNullOrWhiteSpace(message);
        return message.Trim().ToUpperInvariant() == "/STOP";
    }
    public async Task ExecuteAsync(Message message)
    {
        Guard.IsNotNull(message);

        var subscriptionRequest = new SubscriptionRequest
        {
            ChatId = message.Chat.Id,
            LanguageCode = message.From.LanguageCode,
            Operation = SubscriptionRequest.SubscriptionOperation.Remove
        };

        Console.WriteLine($"Received a \"{message.Text}\" message from {message.From.Id} ({message.From.LanguageCode})");
        await _messageQueueService.SendMessageAsync(subscriptionRequest, _subscriptionHandlerQueue);
        Console.WriteLine($"Message from {message.From.Id} forwarded to the {_subscriptionHandlerQueue} queue");
    }
}
