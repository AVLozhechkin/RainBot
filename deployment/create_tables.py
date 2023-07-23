import ydb
import argparse 

argParser = argparse.ArgumentParser()
argParser.add_argument('-t', '--token', help='Your IAM token')
argParser.add_argument('-e', '--endpoint', help='YDB api endpoint')
argParser.add_argument('-d', '--database', help='YDB database path')

args = argParser.parse_args()

with ydb.Driver(connection_string=f'grpcs://{args.endpoint}?database={args.database}',
                credentials=ydb.credentials.AccessTokenCredentials(args.token)) as driver:
    driver.wait(timeout=5)

    subscriptions_table = (
        ydb.TableDescription()
        .with_primary_key('chatId')
        .with_columns(
            ydb.Column('chatId', ydb.PrimitiveType.Int64),
            ydb.Column('languageCode', ydb.OptionalType(ydb.PrimitiveType.Utf8)),
        )
    )

    weather_records_table = (
        ydb.TableDescription()
        .with_primary_keys('dayTime', 'date')
        .with_columns(
            ydb.Column('date', ydb.PrimitiveType.Date),
            ydb.Column('dayTime', ydb.PrimitiveType.Uint8),
            ydb.Column('condition', ydb.OptionalType(ydb.PrimitiveType.Utf8)),
            ydb.Column('isNotified', ydb.OptionalType(ydb.PrimitiveType.Uint8)),
            ydb.Column('precipitationPeriod', ydb.OptionalType(ydb.PrimitiveType.Uint16)),
            ydb.Column('precipitationProbability', ydb.OptionalType(ydb.PrimitiveType.Uint8)),
            ydb.Column('updatedAt', ydb.OptionalType(ydb.PrimitiveType.Datetime)),
        )
    )
    
    session = driver.table_client.session().create()
    session.create_table(f'{args.database}/subscriptions', subscriptions_table)
    session.create_table(f'{args.database}/weatherRecords', weather_records_table)