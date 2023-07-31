using System;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core.Services;
using RainBot.Core;
using Telegram.Bot.Types;
using RainBot.Core.Dto;

namespace RainBot.TelegramHandler.Strategies;
public class InfoStrategy : IMessageProcessStrategy
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly Uri _sendMessageQueue;

    public InfoStrategy(IMessageQueueService messageQueueService, Uri sendMessageQueue)
    {
        _messageQueueService = messageQueueService;
        _sendMessageQueue = sendMessageQueue;
    }
    public bool CanBeExecuted(string message)
    {
        if (message is null)
        {
            return false;
        }

        return message.Trim().ToUpperInvariant() == "/INFO";
    }
    public async Task ExecuteAsync(Message message)
    {
        Guard.IsNotNull(message);

        var sendMessageDto = new SendMessageRequest
        {
            ChatId = message.Chat.Id,
            LanguageCode = message.From.LanguageCode,
            Type = MessageTypes.InfoMessage
        };
        Console.WriteLine($"Received a \"{message.Text}\" message from {message.From.Id} ({message.From.LanguageCode})");
        await _messageQueueService.SendMessageAsync(sendMessageDto, _sendMessageQueue);
    }
}
