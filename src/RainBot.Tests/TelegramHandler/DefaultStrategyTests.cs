using System;
using System.Threading.Tasks;
using Moq;
using RainBot.Core;
using RainBot.Core.Dto;
using RainBot.Core.Services;
using RainBot.TelegramHandler.Strategies;
using Telegram.Bot.Types;
using Xunit;

namespace RainBot.Tests.TelegramHandler;

public class DefaultStrategyTests
{
    [Theory]
    [InlineData(200, "en")]
    [InlineData(200, "ru")]
    public async Task SendsMessage_WhenMessageIsNotNull(long chatId, string languageCode)
    {
        // Arrange
        var messageServiceMock = new Mock<IMessageQueueService>();
        var sendMessageQueue = new Uri("https://test.com");

        var defaultStrategy = new DefaultStrategy(messageServiceMock.Object, sendMessageQueue);
        var message = new Message
        {
            Chat = new Chat { Id = chatId },
            From = new User
            {
                LanguageCode = languageCode,
            }
        };

        // Act
        await defaultStrategy.ExecuteAsync(message);

        // Assert
        messageServiceMock
            .Verify(
            ms => ms.SendMessageAsync(It.Is<SendMessageRequest>(
                smr => smr.ChatId == chatId && smr.LanguageCode == languageCode && smr.Type == MessageTypes.UnknownMessage), sendMessageQueue),
            Times.Once);
    }

    [Fact]
    public async Task ThrowsException_WhenMessageIsNull()
    {
        // Arrange
        var messageServiceMock = new Mock<IMessageQueueService>();
        var sendMessageQueue = new Uri("https://test.com");

        var defaultStrategy = new DefaultStrategy(messageServiceMock.Object, sendMessageQueue);

        // Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => defaultStrategy.ExecuteAsync(null));
    }

    [Theory]
    [InlineData("test")]
    [InlineData("123")]
    [InlineData("")]
    [InlineData(" ")]
    public void CanBeExecuted_AlwaysReturnsTrue(string input)
    {
        // Arrange
        var messageServiceMock = new Mock<IMessageQueueService>();
        var sendMessageQueue = new Uri("https://test.com");

        var defaultStrategy = new DefaultStrategy(messageServiceMock.Object, sendMessageQueue);

        // Act
        var result = defaultStrategy.CanBeExecuted(input);

        // Assert
        Assert.True(result);
    }

}
