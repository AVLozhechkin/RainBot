﻿using System;

namespace RainBot.Core;
public record WeatherRecord
{
    public string Id { get; set; }
    public DateTimeOffset Date { get; set; }
    public DayTime DayTime { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int PrecipitationProbability { get; set; }
    public int PrecipitationPeriod { get; set; }
    public string Condition { get; set; }
}
