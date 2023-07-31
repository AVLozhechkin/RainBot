using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RainBot.Core.Dto;
using RainBot.Core.Models;
using RainBot.Core.Repositories;
using RainBot.Core.Services;

namespace RainBot.ForecastHandler;

public class NotificationService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMessageQueueService _messageQueueService;
    private readonly Uri _sendMessageQueue;
    private readonly string _longitude;
    private readonly string _latitude;

    public NotificationService(ISubscriptionRepository subscriptionRepository, IMessageQueueService messageQueueService, Uri sendMessageQueue, string longitude, string latitude)
    {
        _subscriptionRepository = subscriptionRepository;
        _messageQueueService = messageQueueService;
        _sendMessageQueue = sendMessageQueue;
        _longitude = longitude;
        _latitude = latitude;
    }
    public async Task SendNotifications(IReadOnlyList<Forecast> forecasts)
    {
        var subscriptions = (await _subscriptionRepository.GetSubscriptionsAsync()).ToLookup(s => s.LanguageCode == "en");

        var englishSubscribers = subscriptions[true];

        if (englishSubscribers.Any())
        {
            var message = MessageService.BuildNotificationMessage(forecasts, "en", _longitude, _latitude);

            await SendNotificationsAsync(englishSubscribers, message);
        }

        var russianSubscribers = subscriptions[false];

        if (russianSubscribers.Any())
        {
            var message = MessageService.BuildNotificationMessage(forecasts, "ru", _longitude, _latitude);

            await SendNotificationsAsync(russianSubscribers, message);
        }
    }

    private async Task SendNotificationsAsync(IEnumerable<Subscription> subscriptions, string message)
    {
        foreach (var subscription in subscriptions)
        {
            var sendMessageDto = new SendMessageRequest
            {
                ChatId = subscription.ChatId,
                LanguageCode = subscription.LanguageCode,
                Text = message
            };

            await _messageQueueService.SendMessageAsync(sendMessageDto, _sendMessageQueue);
        }
    }
}
