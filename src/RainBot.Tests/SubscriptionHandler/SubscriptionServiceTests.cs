using RainBot.Core.Services;
using Moq;
using RainBot.SubscriptionHandler;
using System;
using RainBot.Core.Repositories;
using RainBot.Core;
using RainBot.Core.Models;
using System.Threading.Tasks;
using RainBot.Core.Dto;
using Xunit;

namespace RainBot.Tests.SubscriptionHandler;

public class SubscriptionServiceTests
{
    [Theory]
    [InlineData(1234, "ru", QueryResult.Ok, MessageTypes.SubscriptionAdded)]
    [InlineData(4567, "en", QueryResult.Ok, MessageTypes.SubscriptionAdded)]
    [InlineData(1234, "ru", QueryResult.AlreadyExist, MessageTypes.SubscriptionAlreadyExist)]
    [InlineData(4567, "en", QueryResult.AlreadyExist, MessageTypes.SubscriptionAlreadyExist)]
    [InlineData(1234, "ru", QueryResult.SomethingWentWrong, MessageTypes.SomethingWentWrong)]
    [InlineData(4567, "en", QueryResult.SomethingWentWrong, MessageTypes.SomethingWentWrong)]
    public async Task AddSubscription_And_SendReply_When_EverythingIsCorrect(long chatId, string languageCode, QueryResult queryReturn, MessageTypes expectedMessageType)
    {
        // Arrange
        var subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        subscriptionRepositoryMock
            .Setup(sr => sr.AddSubscriptionAsync(It.Is<Subscription>(s => s.ChatId == chatId && s.LanguageCode == languageCode)).Result)
            .Returns(queryReturn);

        var messageServiceMock = new Mock<IMessageQueueService>();

        var sendMessageQueue = new Uri("http://test.com");

        var subscriptionService = new SubscriptionService(subscriptionRepositoryMock.Object, messageServiceMock.Object, sendMessageQueue);

        // Act 
        await subscriptionService.AddSubscription(chatId, languageCode);

        // Assert
        subscriptionRepositoryMock
            .Verify(
                sr => sr.AddSubscriptionAsync(It.Is<Subscription>(s => s.ChatId == chatId && s.LanguageCode == languageCode)),
                Times.Once);

        messageServiceMock
            .Verify(
                ms => ms.SendMessageAsync(
                    It.Is<SendMessageRequest>(smr => smr.ChatId == chatId && smr.LanguageCode == languageCode && smr.Type == expectedMessageType),
                    sendMessageQueue),
                Times.Once);
    }

    [Theory]
    [InlineData(1234, "ru", QueryResult.Ok, MessageTypes.SubscriptionRemoved)]
    [InlineData(4567, "en", QueryResult.Ok, MessageTypes.SubscriptionRemoved)]
    [InlineData(1234, "ru", QueryResult.SomethingWentWrong, MessageTypes.SomethingWentWrong)]
    [InlineData(4567, "en", QueryResult.SomethingWentWrong, MessageTypes.SomethingWentWrong)]
    public async Task RemoveSubscription_And_SendReply_When_EverythingIsCorrect(long chatId, string languageCode, QueryResult queryReturn, MessageTypes expectedMessageType)
    {
        // Arrange
        var subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        subscriptionRepositoryMock
            .Setup(sr => sr.DeleteSubscriptionByChatIdAsync(chatId).Result)
            .Returns(queryReturn);

        var messageServiceMock = new Mock<IMessageQueueService>();

        var sendMessageQueue = new Uri("http://test.com");

        var subscriptionService = new SubscriptionService(subscriptionRepositoryMock.Object, messageServiceMock.Object, sendMessageQueue);

        // Act 
        await subscriptionService.RemoveSubscription(chatId, languageCode);

        // Assert
        subscriptionRepositoryMock
            .Verify(
                sr => sr.DeleteSubscriptionByChatIdAsync(chatId),
                Times.Once);

        messageServiceMock
            .Verify(
                ms => ms.SendMessageAsync(
                    It.Is<SendMessageRequest>(smr => smr.ChatId == chatId && smr.LanguageCode == languageCode && smr.Type == expectedMessageType),
                    sendMessageQueue),
                Times.Once);
    }
}
