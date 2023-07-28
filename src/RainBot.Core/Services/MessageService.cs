using System;
using System.Collections.Generic;
using CommunityToolkit.Diagnostics;
using RainBot.Core.Models;

namespace RainBot.Core.Services;

public static class MessageService
{
    public static string BuildNotificationMessage(IReadOnlyList<Forecast> forecasts, string languageCode = "ru")
    {
        Guard.IsNotNull(forecasts);

        if (forecasts.Count == 1)
        {
            return BuildMessageForSingleRecord(forecasts[0], languageCode);
        }

        if (forecasts.Count == 2 && forecasts[0].Condition == forecasts[1].Condition)
        {
            return BuildMessageForSameConditions(forecasts, languageCode);
        }

        if (forecasts.Count == 2)
        {
            return BuildMessageForDifferentConditions(forecasts, languageCode);
        }

        throw new ArgumentException("Records must contain 1 or 2 records");
    }

    private static string BuildMessageForSameConditions(IReadOnlyList<Forecast> records, string languageCode = "ru")
    {
        Guard.IsNotNull(records);

        if (languageCode == "en")
        {
            return string.Format(
                MessageStrings.EnglishMessages.Value[MessageTypes.WeatherTemplateForSameConditions],
                MessageStrings.EnglishConditions.Value[records[0].Condition],
                MessageStrings.EnglishDayTimes.Value[records[0].DayTime],
                MessageStrings.EnglishDayTimes.Value[records[1].DayTime],
                records[0].PrecipitationPeriod,
                records[1].PrecipitationPeriod
                );
        }

        return string.Format(
                MessageStrings.RussianMessages.Value[MessageTypes.WeatherTemplateForSameConditions],
                MessageStrings.RussianDayTimes.Value[records[0].DayTime],
                MessageStrings.RussianDayTimes.Value[records[1].DayTime],
                MessageStrings.RussianConditions.Value[records[0].Condition],
                records[0].PrecipitationPeriod,
                records[1].PrecipitationPeriod
                );
    }

    private static string BuildMessageForDifferentConditions(IReadOnlyList<Forecast> records, string languageCode = "ru")
    {
        Guard.IsNotNull(records);

        if (languageCode == "en")
        {
            return string.Format(
                MessageStrings.EnglishMessages.Value[MessageTypes.WeatherTemplateForDifferentConditions],
                MessageStrings.EnglishConditions.Value[records[0].Condition],
                MessageStrings.EnglishDayTimes.Value[records[0].DayTime],
                MessageStrings.EnglishConditions.Value[records[1].Condition],
                MessageStrings.EnglishDayTimes.Value[records[1].DayTime],
                records[0].PrecipitationPeriod,
                records[1].PrecipitationPeriod
                );
        }

        return string.Format(
                MessageStrings.RussianMessages.Value[MessageTypes.WeatherTemplateForDifferentConditions],
                MessageStrings.RussianConditions.Value[records[0].Condition],
                MessageStrings.RussianDayTimes.Value[records[0].DayTime],
                MessageStrings.RussianDayTimes.Value[records[1].DayTime],
                MessageStrings.RussianConditions.Value[records[1].Condition],
                records[0].PrecipitationPeriod,
                records[1].PrecipitationPeriod
                );
    }

    private static string BuildMessageForSingleRecord(Forecast record, string languageCode = "ru")
    {
        Guard.IsNotNull(record);

        if (languageCode == "en")
        {
            return string.Format(
                MessageStrings.EnglishMessages.Value[MessageTypes.WeatherTemplateForOneRecord],
                MessageStrings.EnglishConditions.Value[record.Condition],
                MessageStrings.EnglishDayTimes.Value[record.DayTime],
                record.PrecipitationPeriod
                );
        }

        return string.Format(
                MessageStrings.RussianMessages.Value[MessageTypes.WeatherTemplateForOneRecord],
                MessageStrings.RussianDayTimes.Value[record.DayTime],
                MessageStrings.RussianConditions.Value[record.Condition],
                record.PrecipitationPeriod
                );
    }
}
