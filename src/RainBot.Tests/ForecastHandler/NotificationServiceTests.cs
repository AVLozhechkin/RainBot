using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using RainBot.Core.Dto;
using RainBot.Core.Models;
using RainBot.Core.Repositories;
using RainBot.Core.Services;
using RainBot.ForecastHandler;
using RainBot.Tests.Utils;
using Xunit;

namespace RainBot.Tests.ForecastHandler;

public class NotificationServiceTests
{
    [Theory]
    [MemberData(nameof(GetDifferentForecasts))]
    public async Task NotificationService_SendsNotification_WhenDataIsCorrect(IReadOnlyList<Forecast> forecasts, string latitude = null, string longitude = null)
    {
        // Arrange
        var subscriptions = new List<Subscription>
        {
            new Subscription { ChatId = 1, LanguageCode = "ru" },
            new Subscription { ChatId = 2, LanguageCode = "en" },
            new Subscription { ChatId = 3, LanguageCode = "ru" },
            new Subscription { ChatId = 4, LanguageCode = "en" },
        };
        var subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        subscriptionRepositoryMock.Setup(sr => sr.GetSubscriptionsAsync()).ReturnsAsync(subscriptions);

        var messageQueueServiceMock = new Mock<IMessageQueueService>();

        var sendMessageQueue = new Uri("https://test.com");

        var notificationService = new NotificationService(subscriptionRepositoryMock.Object, messageQueueServiceMock.Object, sendMessageQueue, longitude, latitude);

        // Act
        await notificationService.SendNotifications(forecasts);

        // Assert
        foreach (var subscription in subscriptions)
        {
            messageQueueServiceMock
                .Verify(
                mqs => mqs.SendMessageAsync(It.Is<SendMessageRequest>(
                    smr => smr.ChatId == subscription.ChatId &&
                    smr.LanguageCode == subscription.LanguageCode &&
                    smr.Text == MessageService.BuildNotificationMessage(forecasts, subscription.LanguageCode, longitude, latitude)), sendMessageQueue),
                Times.Once);
        }
    }

    public static IEnumerable<object[]> GetDifferentForecasts()
    {
        return new List<object[]>()
        {
            new object[] { ForecastGenerator.GenerateRainyForecats(1), "59.938951", "30.315635" },
            new object[] { ForecastGenerator.GenerateRainyForecats(2), "59.938951", "30.315635" },
            new object[] { ForecastGenerator.GenerateRainyForecats(2, true), "59.938951", "30.315635" },
            new object[] { ForecastGenerator.GenerateRainyForecats(1) },
            new object[] { ForecastGenerator.GenerateRainyForecats(2) },
            new object[] { ForecastGenerator.GenerateRainyForecats(2, true) },
        };
    }

}
