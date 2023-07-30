using System;
using System.Collections.Generic;
using RainBot.Core.Models;

namespace RainBot.Core;

public static class MessageStrings
{
    private static string s_yandexUri = $"https://yandex.ru/pogoda/?lat={Environment.GetEnvironmentVariable("LATITUDE")}&lon={Environment.GetEnvironmentVariable("LONGITUDE")}";

    public static readonly Lazy<Dictionary<MessageTypes, string>> EnglishMessages = new Lazy<Dictionary<MessageTypes, string>>(
        () => new Dictionary<MessageTypes, string>
        {
            { MessageTypes.SomethingWentWrong, "Something went wrong. Try to repeat your request later." },
            { MessageTypes.SubscriptionAdded, "The subscription was successfully added. Every time rain appears in the forecasts, I will send notifications about it." },
            { MessageTypes.SubscriptionAlreadyExist, "The subscription has already been issued." },
            { MessageTypes.SubscriptionRemoved, "The subscription was successfully deleted. All data that you used the bot has been deleted." },
            { MessageTypes.UnknownMessage, "If you want to unsubscribe, please send \"/stop\" message." },
            { MessageTypes.WeatherTemplateForSameConditions, $"[According to Yandex Weather,]({s_yandexUri})" + " {0} is expected in the {1} and {2}. The approximate duration of precipitation is {3} minutes and {4} minutes, respectively." },
            { MessageTypes.WeatherTemplateForDifferentConditions, $"[According to Yandex Weather,]({s_yandexUri})" + " {0} is expected in the {1}, and {2} in the {3}. The approximate duration of precipitation is {4} and {5} minutes, respectively." },
            { MessageTypes.WeatherTemplateForOneRecord, $"[According to Yandex Weather,]({s_yandexUri})" + " {0} is expected in the {1}. The approximate duration of precipitation is {2} minutes." },
        }
        );

    public static readonly Lazy<Dictionary<MessageTypes, string>> RussianMessages = new Lazy<Dictionary<MessageTypes, string>>(
        () => new Dictionary<MessageTypes, string>
        {
            { MessageTypes.SomethingWentWrong, "Что-то пошло не так. Попробуйте повторить свой запрос позже." },
            { MessageTypes.SubscriptionAdded, "Подписка успешно добавлена. Каждый раз, когда в прогнозах будет появляться дождь - я буду присылать уведомления об этом." },
            { MessageTypes.SubscriptionAlreadyExist, "Подписка уже оформлена." },
            { MessageTypes.SubscriptionRemoved, "Подписка успешно удалена. Все данные о том, что вы пользовались ботом были удалены." },
            { MessageTypes.UnknownMessage, "Если вы хотите отписаться, то пришлите сообщение с текстом \"/stop\"" },
            { MessageTypes.WeatherTemplateForSameConditions, $"[По данным Яндекс Погоды,]({s_yandexUri})" + " {0} и {1} ожидается {2}. Приблизительная продолжительность осадков - {3} и {4} мин. соответственно." },
            { MessageTypes.WeatherTemplateForDifferentConditions, $"[По данным Яндекс Погоды,]({s_yandexUri})" + " {0} ожидается {1}, а {2} - {3}. Приблизительная продолжительность осадков - {4} и {5} мин. соответственно." },
            { MessageTypes.WeatherTemplateForOneRecord, $"[По данным Яндекс Погоды,]({s_yandexUri})" + " {0} ожидается {1}. Приблизительная продолжительность осадков - {2} мин." },
        }
        );


    public static readonly Lazy<Dictionary<string, string>> EnglishConditions = new Lazy<Dictionary<string, string>>(
       () => new Dictionary<string, string>
       {
           { "light-rain", "light rain" },
           { "rain", "rain" },
           { "heavy-rain", "heavy rain" },
           { "showers", "rainfall" },
           { "wet-snow", "sleet" },
           { "light-snow", "light snow" },
           { "snow", "snow" },
           { "snow-showers", "snowfall" },
           { "hail", "hail" },
           { "thunderstorm", "thunderstorm" },
           { "thunderstorm-with-rain ", "thundery rain" },
           { "thunderstorm-with-hail", "hailstorm" },
       });

    public static readonly Lazy<Dictionary<string, string>> RussianConditions = new Lazy<Dictionary<string, string>>(
       () => new Dictionary<string, string>
       {
           { "light-rain", "небольшой дождь" },
           { "rain", "дождь" },
           { "heavy-rain", "сильный дождь" },
           { "showers", "ливень" },
           { "wet-snow", "дождь со снегом" },
           { "light-snow", "небольшой снег" },
           { "snow", "снег" },
           { "snow-showers", "снегопад" },
           { "hail", "град" },
           { "thunderstorm ", "гроза" },
           { "thunderstorm-with-rain ", "дождь с грозой" },
           { "thunderstorm-with-hail", "гроза с градом" }
       });

    public static readonly Lazy<Dictionary<DayTime, string>> RussianDayTimes = new Lazy<Dictionary<DayTime, string>>(
       () => new Dictionary<DayTime, string>
       {
           { DayTime.Morning, "утром" },
           { DayTime.Day, "днём" },
           { DayTime.Evening, "вечером" },
           { DayTime.Night, "ночью" },
       });

    public static readonly Lazy<Dictionary<DayTime, string>> EnglishDayTimes = new Lazy<Dictionary<DayTime, string>>(
       () => new Dictionary<DayTime, string>
       {
           { DayTime.Morning, "morning" },
           { DayTime.Day, "afternoon" },
           { DayTime.Evening, "evening" },
           { DayTime.Night, "night" },
       });
}
