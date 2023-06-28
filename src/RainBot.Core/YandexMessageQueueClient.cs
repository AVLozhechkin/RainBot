// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Amazon.Runtime;
using Amazon.SQS;

namespace RainBot.Core;

public class YandexMessageQueueClient : IDisposable
{
    private readonly AmazonSQSClient _client;
    private bool _disposedValue;

    public YandexMessageQueueClient(string accessKey, string secret, string endpointRegion)
    {
        var basicAWSCredentials = new BasicAWSCredentials(accessKey, secret);
        var amazonConfig = new AmazonSQSConfig { ServiceURL = "https://message-queue.api.cloud.yandex.net", AuthenticationRegion = endpointRegion };
        _client = new AmazonSQSClient(basicAWSCredentials, amazonConfig);
    }

    public async Task SendMessageAsync(object message, Uri queueUrl, bool shouldBeSerialized = false)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }
        if (queueUrl is null)
        {
            throw new ArgumentNullException(nameof(queueUrl));
        }
        string serializedMessage = shouldBeSerialized ? JsonSerializer.Serialize(message) : message.ToString();
        await _client.SendMessageAsync(queueUrl.AbsoluteUri, serializedMessage).ConfigureAwait(false);
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
