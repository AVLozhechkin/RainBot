using System.Text.Json.Serialization;

namespace RainBot.Core.Models;

public record ServiceToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}
