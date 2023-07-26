using System;
using System.Threading.Tasks;

namespace RainBot.Core.Services;

public interface IMessageQueueService : IDisposable
{
    Task SendMessageAsync(string message, Uri queueUrl);
    Task SendMessageAsync(object message, Uri queueUrl);
}
