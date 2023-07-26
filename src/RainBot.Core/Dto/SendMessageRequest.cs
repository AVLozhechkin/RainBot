namespace RainBot.Core.Dto;

public record SendMessageRequest
{
    public long ChatId { get; set; }
    public string LanguageCode { get; set; }
    public string Text { get; set; }
    public MessageTypes Type { get; set; }
}
