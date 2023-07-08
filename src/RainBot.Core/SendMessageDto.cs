using System;

namespace RainBot.Core;

public record SendMessageDto
{
    public long ChatId { get; set; }
    public string LanguageCode { get; set; }
    public string Text { get; set; }
    public MessageTypes Type { get; set; }
}
