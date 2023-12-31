using System;

namespace RainBot.Core.Models;

public record Forecast
{
    public DateTimeOffset Date { get; set; }
    public DayTime DayTime { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public byte PrecipitationProbability { get; set; }
    public ushort PrecipitationPeriod { get; set; }
    public string Condition { get; set; }
    public bool IsNotified { get; set; }
}
