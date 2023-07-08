using System.Collections.Generic;
using System.Text.Json.Serialization;
using RainBot.Core;

namespace RainBot.NotificationHandler;

public record Request
{
    [JsonPropertyName("messages")]
    public IReadOnlyList<Message> Messages { get; set; }
}

public record Message
{
    [JsonPropertyName("details")]
    public Details Details { get; set; }
}

public record Details
{
    [JsonPropertyName("message")]
    public MessageBody Message { get; set; }
}

public record MessageBody
{
    [JsonPropertyName("body")]
    public QueueInput Body { get; set; }
}
public record QueueInput
{
    public IReadOnlyList<WeatherRecord> WeatherRecords { get; set; }
}
