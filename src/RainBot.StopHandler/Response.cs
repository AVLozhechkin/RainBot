using System.Text.Json.Serialization;

namespace RainBot.StopHandler;

public record Response
{
    public Response(int statusCode, string body)
    {
        StatusCode = statusCode;
        Body = body;
    }

    [JsonPropertyName("body")]
    public string Body { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}
