using System.Threading.Tasks;

namespace RainBot.SubscriptionHandler;

public interface ISubscriptionService
{
    public Task AddSubscription(long chatId, string languageCode);
    public Task RemoveSubscription(long chatId, string languageCode);
}
