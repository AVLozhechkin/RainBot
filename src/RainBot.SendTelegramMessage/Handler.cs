using System;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using RainBot.Core.Dto;
using RainBot.Core.Models.Functions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RainBot.SendTelegramMessage;

public class Handler
{
    public async Task<Response> FunctionHandler(QueueRequest request)
    {
        Guard.IsNotNull(request);

        var telegramToken = Environment.GetEnvironmentVariable("TG_TOKEN");
        Guard.IsNotNullOrWhiteSpace(telegramToken);

        var message = JsonSerializer.Deserialize<SendMessageRequest>(request.Messages[0].Details.Message.Body);
        Guard.IsNotNull(message);
        Guard.IsNotEqualTo(message.ChatId, 0);

        var text = message.Text switch
        {
            null => GetMessageText(message),
            _ => message.Text
        };

        var botClient = new TelegramBotClient(telegramToken);
        await botClient.SendTextMessageAsync(new ChatId(message.ChatId), text, parseMode: ParseMode.Markdown, disableWebPagePreview: true);

        return new Response(200, string.Empty);
    }

    private static string GetMessageText(SendMessageRequest message) => message.LanguageCode switch
    {
        "en" => MessageStrings.EnglishMessages.Value[message.Type],
        "ru" => MessageStrings.RussianMessages.Value[message.Type],
        _ => MessageStrings.EnglishMessages.Value[message.Type],
    };
}
