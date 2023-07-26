using System;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using RainBot.Core.Dto;
using RainBot.Core.Services;
using Telegram.Bot.Types;

namespace RainBot.TelegramHandler.Strategies;

public class DefaultStrategy : IMessageProcessStrategy
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly Uri _sendMessageQueue;

    public DefaultStrategy(IMessageQueueService messageQueueService, Uri sendMessageQueue)
    {
        _messageQueueService = messageQueueService;
        _sendMessageQueue = sendMessageQueue;
    }
    public bool CanBeExecuted(string message) => true;
    public async Task ExecuteAsync(Message message)
    {
        Guard.IsNotNull(message);

        var sendMessageDto = new SendMessageRequest
        {
            ChatId = message.Chat.Id,
            LanguageCode = message.From.LanguageCode,
            Type = MessageTypes.UnknownMessage
        };
        Console.WriteLine($"Received a \"{message.Text}\" message from {message.From.Id} ({message.From.LanguageCode})");
        await _messageQueueService.SendMessageAsync(sendMessageDto, _sendMessageQueue);
        Console.WriteLine($"Message from {message.From.Id} forwarded to the {_sendMessageQueue} queue");
    }
}
