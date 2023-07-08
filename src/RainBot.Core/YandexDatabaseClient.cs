using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Ydb.Sdk;
using Ydb.Sdk.Auth;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

namespace RainBot.Core;
public class YandexDatabaseClient : IDisposable
{
    private readonly Driver _driver;
    private bool _disposedValue;

    public YandexDatabaseClient(string database, string token)
    {
        Guard.IsNotNullOrWhiteSpace(database);
        Guard.IsNotNullOrWhiteSpace(token);

        var config = new DriverConfig(
            endpoint: "grpcs://ydb.serverless.yandexcloud.net:2135",
            database,
            credentials: new TokenProvider(token)
        );

        _driver = new Driver(
            config: config
        );
    }

    public async Task Initialize() => await _driver.Initialize();

    #region Subcsriptions

    public async Task<IReadOnlyList<Subscription>> GetSubscriptionsAsync()
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
SELECT chatId, languageCode
FROM Subscriptions
";

        var response = await tableClient.SessionExec(async session =>
        {
            return await session.ExecuteDataQuery(
                query: query,
                txControl: TxControl.BeginSerializableRW().Commit()
            );
        });

        response.Status.EnsureSuccess();

        var queryResponse = (ExecuteDataQueryResponse)response;

        var subscriptions = new List<Subscription>();


        foreach (var row in queryResponse.Result.ResultSets[0].Rows)
        {
            var subscription = new Subscription
            {
                ChatId = (long) row["chatId"],
                LanguageCode = row["languageCode"].ToString()
            };

            subscriptions.Add(subscription);
        }

        return subscriptions;
    }

    public async Task<YdbQueryResult> InsertSubscriptionAsync(long chatId, string languageCode)
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
DECLARE $chatId AS Int64;
DECLARE $languageCode as Utf8;

INSERT INTO subscriptions (chatId, languageCode)
VALUES ($chatId, $languageCode)
";

        var response = await tableClient.SessionExec(async session =>
        {
            return await session.ExecuteDataQuery(
                query: query,
                parameters: new Dictionary<string, YdbValue>
                {
                        { "$chatId", YdbValue.MakeInt64(chatId) },
                        { "$languageCode", YdbValue.MakeUtf8(languageCode) }
                },
                txControl: TxControl.BeginSerializableRW().Commit()
            );
        });

        if (response.Status.StatusCode == StatusCode.PreconditionFailed)
        {
            return YdbQueryResult.AlreadyExist;
        }

        return YdbQueryResult.Ok;
    }

    public async Task<YdbQueryResult> RemoveSubscriptionAsync(ulong chatId)
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
DECLARE $chatId AS Uint64;

DELETE FROM subscriptions
WHERE chatId == $chatId
";
        var response = await tableClient.SessionExec(async session =>
        {
            return await session.ExecuteDataQuery(
                query: query,
                parameters: new Dictionary<string, YdbValue>
                {
                        { "$chatId", YdbValue.MakeUint64(chatId) }
                },
                txControl: TxControl.BeginSerializableRW().Commit()
            );
        });

        response.Status.EnsureSuccess();

        return YdbQueryResult.Ok;
    }

    #endregion

    #region WeatherRecords

    public async Task<IReadOnlyList<WeatherRecord>> GetWeatherRecordsByDateAndDayTime(IReadOnlyList<WeatherRecord> weatherRecords)
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
DECLARE $date1 AS Date;
DECLARE $dayTime1 as Utf8;
DECLARE $date2 AS Date;
DECLARE $dayTime1 as Utf8;

SELECT * FROM weatherRecords
WHERE Date = $date1 and DayTime = $dayTime1 or Date = $date2 and DayTime = $dayTime2
";

        var response = await tableClient.SessionExec(async session =>
        {
            return await session.ExecuteDataQuery(
                query: query,
                parameters: new Dictionary<string, YdbValue>
                {
                        { "$date1", YdbValue.MakeDate(weatherRecords[0].Date.DateTime) },
                        { "$dayTime1", YdbValue.MakeUtf8(weatherRecords[0].DayTime.ToString()) },
                        { "$date2", YdbValue.MakeDate(weatherRecords[1].Date.DateTime) },
                        { "$dayTime2", YdbValue.MakeUtf8(weatherRecords[1].DayTime.ToString()) }
                },
                txControl: TxControl.BeginSerializableRW().Commit()
            );
        });

        response.Status.EnsureSuccess();

        var queryResponse = (ExecuteDataQueryResponse)response;

        var result = queryResponse.Result.ResultSets[0].Rows.SingleOrDefault();

        if (result is null)
            return null;

        var recordsFromDatabase = new List<WeatherRecord>();

        foreach (var row in queryResponse.Result.ResultSets[0].Rows)
        {
            var weatherRecord = new WeatherRecord
            {
                Date = DateTime.Parse(row["date"].ToString()),
                DayTime = Enum.Parse<DayTime>(row["dayTime"].ToString()),
                Condition = row["condition"].ToString(),
                IsNotified = row["isNotified"].GetUint8() == 1,
                PrecipitationPeriod = (ushort)row["PrecipitationPeriod"],
                PrecipitationProbability = (byte)row["PrecipitationProbability"],
                UpdatedAt = DateTime.Parse(row["UpdatedAt"].ToString()),
            };

            recordsFromDatabase.Add(weatherRecord);
        }

        return recordsFromDatabase;
    }

    public async Task UpsertWeatherRecords(IReadOnlyList<WeatherRecord> weatherRecords)
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
DECLARE $weatherRecords AS List<Struct<
    date: Date;
    dayTime: Utf8;
    condition: Utf8;
    isNotified: Uint8;
    precipitationPeriod: Uint16;
    precipitationProbability: Uint8;
    updatedAt: Date>>;

UPSERT INTO weatherRecords
SELECT * FROM AS_TABLE($weatherRecords);
    ";
        var weatherRecordsData = weatherRecords.Select(wr => YdbValue.MakeStruct(new Dictionary<string, YdbValue>
        {
            { "date", YdbValue.MakeDate(wr.Date.Date) },
            { "dayTime", YdbValue.MakeUtf8(wr.DayTime.ToString()) },
            { "condition", YdbValue.MakeUtf8(wr.Condition) },
            { "isNotified", YdbValue.MakeUint8((byte)(wr.IsNotified ? 1 : 0)) },
            { "precipitationPeriod", YdbValue.MakeUint16(wr.PrecipitationPeriod) },
            { "precipitationProbability", YdbValue.MakeUint8(wr.PrecipitationProbability) },
            { "updatedAt", YdbValue.MakeDate(wr.UpdatedAt.Date) },
        })).ToList();

        var wrList = new Dictionary<string, YdbValue>()
        {
            { "$weatherRecords", YdbValue.MakeList(weatherRecordsData) },
        };

        var response = await tableClient.SessionExec(async session =>
        {
            return await session.ExecuteDataQuery(
            query: query,
            parameters: wrList,
            txControl: TxControl.BeginSerializableRW().Commit()
        );
        });

        response.Status.EnsureSuccess();
    }

    #endregion

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _driver.Dispose();
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

public enum YdbQueryResult
{
    Ok,
    AlreadyExist,
    SomethingWentWrong
}

