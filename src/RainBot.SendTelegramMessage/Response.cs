namespace RainBot.SendTelegramMessage;

public record Response
{
    public Response(int statusCode, string body)
    {
        this.StatusCode = statusCode;
        this.Body = body;
    }
    public string Body { get; set; }
    public int StatusCode { get; set; }
}