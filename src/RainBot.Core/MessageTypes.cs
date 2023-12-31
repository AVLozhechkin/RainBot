﻿namespace RainBot.Core;

public enum MessageTypes
{
    SubscriptionAlreadyExist,
    SubscriptionRemoved,
    SubscriptionAdded,
    SomethingWentWrong,
    UnknownMessage,
    WeatherTemplateForOneRecord,
    WeatherTemplateForSameConditions,
    WeatherTemplateForDifferentConditions,
    InfoMessage
}
