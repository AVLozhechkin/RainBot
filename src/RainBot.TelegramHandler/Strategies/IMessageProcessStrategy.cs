using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace RainBot.TelegramHandler.Strategies;

public interface IMessageProcessStrategy
{
    bool CanBeExecuted(string message);
    Task ExecuteAsync(Message message);
}
