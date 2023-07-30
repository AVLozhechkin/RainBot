locals {
  sqs_endpoint_region = "ru-central1"
}

resource "random_string" "user-hash" {
  length = 15
}

resource "yandex_function" "yandex-weather-fetcher" {
  name               = "yandex-weather-fetcher"
  user_hash          = random_string.user-hash.result
  description        = "Function that fetches forecasts from Yandex.Weather"
  runtime            = "dotnet6"
  entrypoint         = "RainBot.YandexWeatherFetcher.Handler"
  memory             = 128
  execution_timeout  = 5
  service_account_id = yandex_iam_service_account.service_account.id
  tags               = ["latest"]
  environment = {
    "YANDEX_WEATHER_API_KEY" : var.yandex_weather_api_key,
    "SQS_ACCESS_KEY" : yandex_iam_service_account_static_access_key.access_key.access_key,
    "SQS_SECRET" : yandex_iam_service_account_static_access_key.access_key.secret_key,
    "SQS_ENDPOINT_REGION" : local.sqs_endpoint_region,
    "FORECAST_HANDLER_QUEUE" : yandex_message_queue.queue["forecast-handler-queue"].id,
    "LATITUDE": var.latitude,
    "LONGITUDE": var.longitude
  }
  content {
    zip_filename = "../zips/RainBot.YandexWeatherFetcher.zip"
  }
}

resource "yandex_function" "forecast-handler" {
  name               = "forecast-handler"
  user_hash          = random_string.user-hash.result
  description        = "Function that parses weather forecasts, stores it into database and sends notifications if it detects rain."
  runtime            = "dotnet6"
  entrypoint         = "RainBot.ForecastHandler.Handler"
  memory             = 128
  execution_timeout  = 5
  service_account_id = yandex_iam_service_account.service_account.id
  tags               = ["latest"]
  environment = {
    "YDB_DATABASE" : yandex_ydb_database_serverless.rainbot_db.database_path,
    "SQS_ACCESS_KEY" : yandex_iam_service_account_static_access_key.access_key.access_key,
    "SQS_SECRET" : yandex_iam_service_account_static_access_key.access_key.secret_key,
    "SQS_ENDPOINT_REGION" : local.sqs_endpoint_region,
    "SEND_MESSAGE_QUEUE" : yandex_message_queue.queue["send-telegram-message-queue"].id,
    "LATITUDE": var.latitude,
    "LONGITUDE": var.longitude
  }
  content {
    zip_filename = "../zips/RainBot.ForecastHandler.zip"
  }
}

resource "yandex_function" "telegram-handler" {
  name              = "telegram-handler"
  user_hash         = random_string.user-hash.result
  description       = "Handles Telegram webhooks."
  runtime           = "dotnet6"
  entrypoint        = "RainBot.TelegramHandler.Handler"
  memory            = 128
  execution_timeout = 5
  tags              = ["latest"]
  environment = {
    "SQS_ACCESS_KEY" : yandex_iam_service_account_static_access_key.access_key.access_key,
    "SQS_SECRET" : yandex_iam_service_account_static_access_key.access_key.secret_key,
    "SQS_ENDPOINT_REGION" : local.sqs_endpoint_region,
    "SUBSCRIPTION_HANDLER_QUEUE" : yandex_message_queue.queue["subscription-handler-queue"].id,
    "SEND_MESSAGE_QUEUE" : yandex_message_queue.queue["send-telegram-message-queue"].id
  }
  content {
    zip_filename = "../zips/RainBot.TelegramHandler.zip"
  }
}

resource "yandex_function" "subscription-handler" {
  name               = "subscription-handler"
  user_hash          = random_string.user-hash.result
  description        = "Handles /start and /stop messages. Adds and removes subscriptions to/from database."
  runtime            = "dotnet6"
  entrypoint         = "RainBot.SubscriptionHandler.Handler"
  memory             = 128
  execution_timeout  = 5
  service_account_id = yandex_iam_service_account.service_account.id
  tags               = ["latest"]
  environment = {
    "YDB_DATABASE" : yandex_ydb_database_serverless.rainbot_db.database_path,
    "SQS_ACCESS_KEY" : yandex_iam_service_account_static_access_key.access_key.access_key,
    "SQS_SECRET" : yandex_iam_service_account_static_access_key.access_key.secret_key,
    "SQS_ENDPOINT_REGION" : local.sqs_endpoint_region,
    "SEND_MESSAGE_QUEUE" : yandex_message_queue.queue["send-telegram-message-queue"].id,
  }
  content {
    zip_filename = "../zips/RainBot.SubscriptionHandler.zip"
  }
}

resource "yandex_function" "telegram-message-sender" {
  name              = "telegram-message-sender"
  user_hash         = random_string.user-hash.result
  description       = "Sends Telegram message"
  runtime           = "dotnet6"
  entrypoint        = "RainBot.SendTelegramMessage.Handler"
  memory            = 128
  execution_timeout = 5
  tags              = ["latest"]
  environment = {
    "TG_TOKEN" : var.telegram_bot_token
  }
  content {
    zip_filename = "../zips/RainBot.SendTelegramMessage.zip"
  }
}