using System;
using System.Threading.Tasks;
using Moq;
using RainBot.Core.Dto;
using RainBot.Core.Services;
using RainBot.TelegramHandler.Strategies;
using Telegram.Bot.Types;
using Xunit;

namespace RainBot.Tests.TelegramHandler;

public class StopStrategyTests
{
    [Theory]
    [InlineData(200, "en")]
    [InlineData(200, "ru")]
    public async Task SendsMessage_WhenMessageIsNotNull(long chatId, string languageCode)
    {
        // Arrange
        var messageServiceMock = new Mock<IMessageQueueService>();
        var subscriptionHandlerQueue = new Uri("https://test.com");

        var stopStrategy = new StopStrategy(messageServiceMock.Object, subscriptionHandlerQueue);
        var message = new Message
        {
            Chat = new Chat { Id = chatId },
            From = new User
            {
                LanguageCode = languageCode,
            }
        };

        // Act
        await stopStrategy.ExecuteAsync(message);

        // Assert
        messageServiceMock
            .Verify(
            ms => ms.SendMessageAsync(It.Is<SubscriptionRequest>(
                smr => smr.ChatId == chatId && smr.LanguageCode == languageCode && smr.Operation == SubscriptionRequest.SubscriptionOperation.Remove), subscriptionHandlerQueue),
            Times.Once);
    }

    [Fact]
    public async Task ThrowsException_WhenMessageIsNull()
    {
        // Arrange
        var messageServiceMock = new Mock<IMessageQueueService>();
        var subscriptionHandlerQueue = new Uri("https://test.com");

        var stopStrategy = new StopStrategy(messageServiceMock.Object, subscriptionHandlerQueue);

        // Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => stopStrategy.ExecuteAsync(null));
    }


    [Theory]
    [InlineData("/stop", true)]
    [InlineData("/STOP", true)]
    [InlineData("/sToP", true)]
    [InlineData("/start", false)]
    [InlineData("/default", false)]
    [InlineData("any other", false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData(null, false)]
    public void CanBeExecuted_ReturnsTrue_WhenInputIsStart(string input, bool expectedResult)
    {
        // Arrange
        var messageServiceMock = new Mock<IMessageQueueService>();
        var subscriptionHandlerQueue = new Uri("https://test.com");

        var stopStrategy = new StopStrategy(messageServiceMock.Object, subscriptionHandlerQueue);

        // Act
        var result = stopStrategy.CanBeExecuted(input);

        // Assert
        Assert.True(result == expectedResult);
    }
}

