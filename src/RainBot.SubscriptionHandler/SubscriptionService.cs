using System;
using System.Threading.Tasks;
using RainBot.Core;
using RainBot.Core.Dto;
using RainBot.Core.Models;
using RainBot.Core.Repositories;
using RainBot.Core.Services;

namespace RainBot.SubscriptionHandler;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMessageQueueService _mqService;
    private readonly Uri _sendMessageQueue;

    public SubscriptionService(ISubscriptionRepository subscriptionRepository, IMessageQueueService mqService, Uri sendMessageQueue)
    {
        _mqService = mqService;
        _sendMessageQueue = sendMessageQueue;
        _subscriptionRepository = subscriptionRepository;

    }
    public async Task AddSubscription(long chatId, string languageCode)
    {
        var subscription = new Subscription
        {
            ChatId = chatId,
            LanguageCode = languageCode
        };
        Console.WriteLine($"Inserting a subscription for chatId {chatId} ({languageCode})");
        var addResult = await _subscriptionRepository.AddSubscriptionAsync(subscription);
        Console.WriteLine($"Insert result for {chatId} is {addResult}");

        var messageToSend = addResult switch
        {
            QueryResult.Ok => MessageTypes.SubscriptionAdded,
            QueryResult.AlreadyExist => MessageTypes.SubscriptionAlreadyExist,
            _ => MessageTypes.SomethingWentWrong,
        };

        var queueMessage = new SendMessageRequest
        {
            ChatId = chatId,
            LanguageCode = languageCode,
            Type = messageToSend
        };

        await SendMessageAsync(queueMessage, _sendMessageQueue);
    }

    public async Task RemoveSubscription(long chatId, string languageCode)
    {
        Console.WriteLine($"Removing a subscription for chatId {chatId}");
        var removeResult = await _subscriptionRepository.DeleteSubscriptionByChatIdAsync(chatId);
        Console.WriteLine($"Remove result for {chatId} is {removeResult}");

        var messageToSend = removeResult switch
        {
            QueryResult.Ok => MessageTypes.SubscriptionRemoved,
            _ => MessageTypes.SomethingWentWrong,
        };

        var queueMessage = new SendMessageRequest
        {
            ChatId = chatId,
            LanguageCode = languageCode,
            Type = messageToSend
        };

        await SendMessageAsync(queueMessage, _sendMessageQueue);
    }

    private async Task SendMessageAsync(SendMessageRequest message, Uri queue)
    {
        Console.WriteLine($"Sending a message to {message.ChatId} ({message.LanguageCode}) that {message.Type}");
        await _mqService.SendMessageAsync(message, queue);
        Console.WriteLine($"Message for {message.ChatId} ({message.LanguageCode}) forwarded to the {queue} queue");
    }
}