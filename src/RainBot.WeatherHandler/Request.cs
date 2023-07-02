using System.Collections.Generic;
using System.Text.Json.Serialization;
using RainBot.Core;

namespace RainBot.WeatherHandler;

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
    public IReadOnlyList<WeatherRecord> Body { get; set; }
}
