using Ydb.Sdk;
using Ydb.Sdk.Auth;

namespace RainBot.Core;

public static class DriverExtensions
{
    public static Driver Build(string databasePath, string accessToken)
    {
        var config = new DriverConfig(
            endpoint: "grpcs://ydb.serverless.yandexcloud.net:2135",
            databasePath,
            credentials: new TokenProvider(accessToken)
        );

        return new Driver(
            config: config
        );
    }
}
