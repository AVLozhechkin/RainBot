using System.Collections.Generic;

namespace RainBot.YandexWeatherFetcher;

#pragma warning disable IDE1006 // Naming Styles
public record Request
{
    public IReadOnlyCollection<Message> messages { get; set; }
}

public record Details
{
    public MessageBody message { get; set; }
}

public record Message
{
    public Details details { get; set; }
}

public record MessageBody
{
    public string body { get; set; }
}
public record QueueInput
{
    public ulong Id { get; set; }
    public string LanguageCode { get; set; }
}
