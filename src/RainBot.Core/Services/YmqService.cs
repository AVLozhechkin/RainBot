using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SQS;
using CommunityToolkit.Diagnostics;
using RainBot.Core.Services;

namespace RainBot.Core;

public class YmqService : IMessageQueueService
{
    private readonly AmazonSQSClient _client;
    private bool _disposedValue;

    public YmqService(string accessKey, string secret, string endpointRegion)
    {
        Guard.IsNotNullOrWhiteSpace(accessKey);
        Guard.IsNotNullOrWhiteSpace(secret);
        Guard.IsNotNullOrWhiteSpace(endpointRegion);

        var basicAWSCredentials = new BasicAWSCredentials(accessKey, secret);
        var amazonConfig = new AmazonSQSConfig { ServiceURL = "https://message-queue.api.cloud.yandex.net", AuthenticationRegion = endpointRegion };
        _client = new AmazonSQSClient(basicAWSCredentials, amazonConfig);
    }

    public async Task SendMessageAsync(string message, Uri queueUrl)
    {
        Guard.IsNotNullOrWhiteSpace(message);
        Guard.IsNotNull(queueUrl);

        await _client.SendMessageAsync(queueUrl.AbsoluteUri, message);
    }
    public async Task SendMessageAsync(object message, Uri queueUrl)
    {
        Guard.IsNotNull(message);
        Guard.IsNotNull(queueUrl);

        string serializedMessage = JsonSerializer.Serialize(message);
        await _client.SendMessageAsync(queueUrl.AbsoluteUri, serializedMessage);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _client.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
