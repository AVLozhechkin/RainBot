namespace RainBot.TelegramHandler;

public record Request
{
    public string httpMethod { get; set; }
    public string body { get; set; }
}
