namespace RainBot.Core;
public record SendMessageDto
{
    public ulong ChatId { get; set; }
    public MessageTypes Type { get; set; }
    public string LanguageCode { get; set; }
}
