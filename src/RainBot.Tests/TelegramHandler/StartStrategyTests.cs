using System;
using System.Threading.Tasks;
using Moq;
using RainBot.Core.Dto;
using RainBot.Core.Services;
using RainBot.TelegramHandler.Strategies;
using Telegram.Bot.Types;
using Xunit;

namespace RainBot.Tests.TelegramHandler;

public class StartStrategyTests
{
    [Theory]
    [InlineData(200, "en")]
    [InlineData(200, "ru")]
    public async Task SendsMessage_WhenMessageIsNotNull(long chatId, string languageCode)
    {
        // Arrange
        var messageServiceMock = new Mock<IMessageQueueService>();
        var subscriptionHandlerQueue = new Uri("https://test.com");

        var startStrategy = new StartStrategy(messageServiceMock.Object, subscriptionHandlerQueue);
        var message = new Message
        {
            Chat = new Chat { Id = chatId },
            From = new User
            {
                LanguageCode = languageCode,
            }
        };

        // Act
        await startStrategy.ExecuteAsync(message);

        // Assert
        messageServiceMock
            .Verify(
            ms => ms.SendMessageAsync(It.Is<SubscriptionRequest>(
                smr => smr.ChatId == chatId && smr.LanguageCode == languageCode && smr.Operation == SubscriptionRequest.SubscriptionOperation.Add), subscriptionHandlerQueue),
            Times.Once);
    }

    [Fact]
    public async Task ThrowsException_WhenMessageIsNull()
    {
        // Arrange
        var messageServiceMock = new Mock<IMessageQueueService>();
        var subscriptionHandlerQueue = new Uri("https://test.com");

        var startStrategy = new StartStrategy(messageServiceMock.Object, subscriptionHandlerQueue);

        // Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => startStrategy.ExecuteAsync(null));
    }


    [Theory]
    [InlineData("/start", true)]
    [InlineData("/START", true)]
    [InlineData("/sTaRt", true)]
    [InlineData("/stop", false)]
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

        var startStrategy = new StartStrategy(messageServiceMock.Object, subscriptionHandlerQueue);

        // Act
        var result = startStrategy.CanBeExecuted(input);

        // Assert
        Assert.True(result == expectedResult);
    }
}
