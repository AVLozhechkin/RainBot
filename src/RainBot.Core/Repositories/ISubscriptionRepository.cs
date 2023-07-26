using System.Collections.Generic;
using System.Threading.Tasks;
using RainBot.Core.Models;

namespace RainBot.Core.Repositories;

public interface ISubscriptionRepository
{
    Task<QueryResult> AddSubscriptionAsync(Subscription subscription);
    Task<QueryResult> DeleteSubscriptionByChatIdAsync(long chatId);
    Task<IReadOnlyList<Subscription>> GetSubscriptionsAsync();
}
