using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using RainBot.Core.Dto;
using RainBot.Core.Models;
using RainBot.Core.Repositories;
using RainBot.Core.Services;
using RainBot.ForecastHandler;
using Xunit;

namespace RainBot.Tests.ForecastHandler;

public class NotificationServiceTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task NotificationService_SendsNotification_WhenDataIsCorrect(int forecastNumber)
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

        var notificationService = new NotificationService(subscriptionRepositoryMock.Object, messageQueueServiceMock.Object, sendMessageQueue);
        var forecasts = GenerateRainyForecats(forecastNumber);

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
                    smr.Text == MessageService.BuildNotificationMessage(forecasts, subscription.LanguageCode)), sendMessageQueue),
                Times.Once);
        }
    }

    private static List<Forecast> GenerateRainyForecats(int count)
    {
        var forecasts = new List<Forecast>(count);
        var datetime = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            forecasts.Add(new Forecast { Date = datetime.Date, Condition = "rain", DayTime = DayTime.Morning, IsNotified = true, PrecipitationPeriod = 480, PrecipitationProbability = 50, UpdatedAt = datetime });
        }

        return forecasts;
    }
}
