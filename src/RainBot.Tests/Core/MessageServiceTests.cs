using System;
using System.Collections.Generic;
using RainBot.Core.Models;
using RainBot.Core.Services;
using RainBot.Tests.Utils;
using Xunit;

namespace RainBot.Tests.Core;

public class MessageServiceTests
{
    [Fact]
    public void MessageService_ThrowsArgumentNullException_WhenForecastsAreNull()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => MessageService.BuildNotificationMessage(null, "ru"));
    }

    [Theory]
    [MemberData(nameof(GetMessageServiceInputs))]
    public void MessageService_BuildsNotificationMessages_WhenInputIsCorrect(IReadOnlyList<Forecast> forecasts, string languageCode, string expectedMessage)
    {
        // Arrange

        // Act
        var message = MessageService.BuildNotificationMessage(forecasts, languageCode);

        // Assert
        Assert.Equal(expectedMessage, message);
    }

    public static IEnumerable<object[]> GetMessageServiceInputs() => new List<object[]>
    {
        new object[]
        {
            ForecastGenerator.GenerateForecasts("r_", true),
            "ru",
            "По данным Яндекс Погоды, ночью ожидается дождь. Приблизительная продолжительность осадков - 480 мин."
        },
        new object[]
        {
            ForecastGenerator.GenerateForecasts("_r", true),
            "ru",
            "По данным Яндекс Погоды, утром ожидается дождь. Приблизительная продолжительность осадков - 480 мин."
        },
        new object[]
        {
            ForecastGenerator.GenerateForecasts("rr", true),
            "ru",
            "По данным Яндекс Погоды, ночью и утром ожидается дождь. Приблизительная продолжительность осадков - 480 и 480 мин. соответственно."
        },
    };
}

