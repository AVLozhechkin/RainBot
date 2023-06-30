using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using Ydb.Sdk;
using Ydb.Sdk.Auth;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

namespace RainBot.Core;
public class YandexDatabaseService : IDisposable
{
    private readonly Driver _driver;
    private bool _disposedValue;

    public YandexDatabaseService(string database, string token)
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

    public async Task<YdbQueryResult> InsertSubscriptionAsync(ulong chatId, string languageCode)
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
DECLARE $chatId AS Uint64;
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
                        { "$chatId", YdbValue.MakeUint64(chatId) },
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

    public async Task<YdbQueryResult> RemoveSubscription(ulong chatId)
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

   /* #region WeatherRecords

    public async Task<Forecast> GetWeatherRecordByDateAndDayTime(DateTime date, DayTime dayTime)
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());

        var query = @"
DECLARE $date AS Date;
DECLARE $dayTime as Utf8;

SELECT * FROM weatherRecords
WHERE Date = $date and DayTime = $dayTime
";
        var response = await tableClient.SessionExec(async session =>
        {
            return await session.ExecuteDataQuery(
                query: query,
                parameters: new Dictionary<string, YdbValue>
                {
                        { "$date", YdbValue.MakeDate(date) },
                        { "$dayTime", YdbValue.MakeUtf8(dayTime.ToString()) },
                },
                txControl: TxControl.BeginSerializableRW().Commit()
            );
        });

        response.Status.EnsureSuccess();

        var queryResponse = (ExecuteDataQueryResponse)response;
        var result = queryResponse.Result.ResultSets[0].Rows.SingleOrDefault();

        if (result is null)
            return null;

        var forecast = new Forecast
        {
            Id = (string)result["Id"],
            Date = DateTime.Parse(result["Date"].ToString()),
            DayTime = Enum.Parse<DayTime>(result["DayTime"].ToString()),
            PrecipitationPeriod = (int)result["PrecipitationPeriod"],
            PrecipitationProbability = (int)result["PrecipitationProbability"],
            UpdatedAt = DateTime.Parse(result["UpdatedAt"].ToString()),

        };

        return forecast;
    }
    
*//*        public async Task UpdateWeatherRecord(Forecast forecast)
        {
            using var tableClient = new TableClient(driver, new TableClientConfig());

            var query = @"
    DECLARE $id as Utf8;
    DECLARE $PrecipitationPeriod as 

    UPDATE streamers
    SET isEnabled = false
    WHERE id = $id
    ";

        }*//*

    #endregion*/

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

