using System.Text.Json.Serialization;

namespace RainBot.Core.Models.Functions;

public record Request
{
    [JsonPropertyName("httpMethod")]
    public string httpMethod { get; set; }

    [JsonPropertyName("body")]
    public string body { get; set; }
}
