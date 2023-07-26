using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RainBot.Core.Models;
using RainBot.Core.Repositories;
using Ydb.Sdk;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

namespace RainBot.Core;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly Driver _driver;

    public SubscriptionRepository(Driver driver)
    {
        _driver = driver;
    }

    public async Task<QueryResult> AddSubscriptionAsync(Subscription subscription)
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
DECLARE $chatId AS Int64;
DECLARE $languageCode as Utf8;

INSERT INTO subscriptions (chatId, languageCode)
VALUES ($chatId, $languageCode)
";

        var sessionResult = await tableClient.SessionExec(async session =>
        {
            return await session.ExecuteDataQuery(
                query: query,
                parameters: new Dictionary<string, YdbValue>
                {
                        { "$chatId", YdbValue.MakeInt64(subscription.ChatId) },
                        { "$languageCode", YdbValue.MakeUtf8(subscription.LanguageCode) }
                },
                txControl: TxControl.BeginSerializableRW().Commit()
            );
        });

        if (sessionResult.Status.IsSuccess)
        {
            return QueryResult.Ok;
        }

        if (sessionResult.Status.StatusCode == StatusCode.PreconditionFailed)
        {
            return QueryResult.AlreadyExist;
        }

        return QueryResult.SomethingWentWrong;
    }
    public async Task<QueryResult> DeleteSubscriptionByChatIdAsync(long chatId)
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
DECLARE $chatId AS Int64;

DELETE FROM subscriptions
WHERE chatId == $chatId
";
        var sessionResult = await tableClient.SessionExec(async session =>
        {
            return await session.ExecuteDataQuery(
                query: query,
                parameters: new Dictionary<string, YdbValue>
                {
                        { "$chatId", YdbValue.MakeInt64(chatId) }
                },
                txControl: TxControl.BeginSerializableRW().Commit()
            );
        });

        if (sessionResult.Status.IsSuccess)
        {
            return QueryResult.Ok;
        }

        return QueryResult.SomethingWentWrong;
    }
    public async Task<IReadOnlyList<Subscription>> GetSubscriptionsAsync()
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
SELECT chatId, languageCode
FROM subscriptions
";

        var sessionResult = await tableClient.SessionExec(async session =>
        {
            return await session.ExecuteDataQuery(
                query: query,
                txControl: TxControl.BeginSerializableRW().Commit()
            );
        });

        var response = (ExecuteDataQueryResponse)sessionResult;

        if (response.Result.ResultSets.Count == 0)
        {
            return Array.Empty<Subscription>();
        }

        var subscriptions = new List<Subscription>(response.Result.ResultSets.Count);

        foreach (var row in response.Result.ResultSets[0].Rows)
        {
            var subscription = new Subscription
            {
                ChatId = row["chatId"].GetInt64(),
                LanguageCode = row["languageCode"].GetOptionalUtf8()
            };

            subscriptions.Add(subscription);
        }

        return subscriptions;
    }
}
