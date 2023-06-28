namespace RainBot.YandexWeatherFetcher;

public record Response
{
    public Response(int statusCode, string body)
    {
        this.statusCode = statusCode;
        this.body = body;
    }
    public string body { get; set; }
    public int statusCode { get; set; }
}
