namespace RainBot.Core.Models;

public record Subscription
{
    public long ChatId { get; set; }
    public string LanguageCode { get; set; }
}
