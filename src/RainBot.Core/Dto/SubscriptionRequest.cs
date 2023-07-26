namespace RainBot.Core.Dto;

public record SubscriptionRequest
{
    public SubscriptionOperation Operation { get; set; }
    public long ChatId { get; set; }
    public string LanguageCode { get; set; }

    public enum SubscriptionOperation
    {
        Add,
        Remove
    }
}
