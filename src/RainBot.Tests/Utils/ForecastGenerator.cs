using System;
using System.Collections.Generic;
using RainBot.Core.Models;

namespace RainBot.Tests.Utils;

public static class ForecastGenerator
{
    public static IReadOnlyList<Forecast> GenerateRainyForecats(int count, bool differentConditions = false)
    {
        var forecasts = new List<Forecast>(count);
        var datetime = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            forecasts.Add(new Forecast { Date = datetime.Date, Condition = "rain", DayTime = DayTime.Morning, IsNotified = true, PrecipitationPeriod = 480, PrecipitationProbability = 50, UpdatedAt = datetime });
        }

        if (count == 2 && differentConditions)
        {
            forecasts[1].Condition = "snow";
        }

        return forecasts;
    }
    public static IReadOnlyList<Forecast> GenerateForecasts(string pattern, bool isNotified)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return Array.Empty<Forecast>();
        }

        var list = new List<Forecast>();
        var updatedAt = DateTime.Parse("07/27/2023 00:00:00");
        var date = updatedAt.Date;

        switch (pattern[0])
        {
            case 'c':
                list.Add(new Forecast { Date = date, Condition = "cloudy", DayTime = DayTime.Night, IsNotified = false, PrecipitationPeriod = 480, PrecipitationProbability = 0, UpdatedAt = updatedAt });
                break;
            case 'r':
                list.Add(new Forecast { Date = date, Condition = "rain", DayTime = DayTime.Night, IsNotified = isNotified, PrecipitationPeriod = 480, PrecipitationProbability = 50, UpdatedAt = updatedAt });
                break;
        }

        switch (pattern[1])
        {
            case 'c':
                list.Add(new Forecast { Date = date, Condition = "cloudy", DayTime = DayTime.Morning, IsNotified = false, PrecipitationPeriod = 480, PrecipitationProbability = 0, UpdatedAt = updatedAt });
                break;
            case 'r':
                list.Add(new Forecast { Date = date, Condition = "rain", DayTime = DayTime.Morning, IsNotified = isNotified, PrecipitationPeriod = 480, PrecipitationProbability = 50, UpdatedAt = updatedAt });
                break;
        }

        return list;
    }
}
