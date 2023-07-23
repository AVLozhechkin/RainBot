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
    "WEATHER_QUEUE" : yandex_message_queue.queue["weather-queue"].id
  }
  content {
    zip_filename = "../zips/RainBot.YandexWeatherFetcher.zip"
  }
}

resource "yandex_function" "weather-handler" {
  name               = "weather-handler"
  user_hash          = random_string.user-hash.result
  description        = "Function that parses weather forecasts and stores it into database."
  runtime            = "dotnet6"
  entrypoint         = "RainBot.WeatherHandler.Handler"
  memory             = 128
  execution_timeout  = 5
  service_account_id = yandex_iam_service_account.service_account.id
  tags               = ["latest"]
  environment = {
    "YDB_DATABASE" : yandex_ydb_database_serverless.rainbot_db.database_path,
    "SQS_ACCESS_KEY" : yandex_iam_service_account_static_access_key.access_key.access_key,
    "SQS_SECRET" : yandex_iam_service_account_static_access_key.access_key.secret_key,
    "SQS_ENDPOINT_REGION" : local.sqs_endpoint_region,
    "NOTIFICATION_QUEUE" : yandex_message_queue.queue["notification-queue"].id
  }
  content {
    zip_filename = "../zips/RainBot.WeatherHandler.zip"
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
    "START_QUEUE" : yandex_message_queue.queue["start-handler-queue"].id,
    "STOP_QUEUE" : yandex_message_queue.queue["stop-handler-queue"].id,
    "UNKNOWN_QUEUE" : yandex_message_queue.queue["unknown-queue"].id
  }
  content {
    zip_filename = "../zips/RainBot.TelegramHandler.zip"
  }
}

resource "yandex_function" "stop-handler" {
  name               = "stop-handler"
  user_hash          = random_string.user-hash.result
  description        = "Handles /stop messages. Removes subscription from database."
  runtime            = "dotnet6"
  entrypoint         = "RainBot.StopHandler.Handler"
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
    zip_filename = "../zips/RainBot.StopHandler.zip"
  }
}

resource "yandex_function" "start-handler" {
  name               = "start-handler"
  user_hash          = random_string.user-hash.result
  description        = "Handles /start messages. Adds subscription to database."
  runtime            = "dotnet6"
  entrypoint         = "RainBot.StartHandler.Handler"
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
    zip_filename = "../zips/RainBot.StartHandler.zip"
  }
}

resource "yandex_function" "unknown-message-handler" {
  name              = "unknown-message-handler"
  user_hash         = random_string.user-hash.result
  description       = "Handles unknown messages."
  runtime           = "dotnet6"
  entrypoint        = "RainBot.UnknownMessageHandler.Handler"
  memory            = 128
  execution_timeout = 5
  tags              = ["latest"]
  environment = {
    "SQS_ACCESS_KEY" : yandex_iam_service_account_static_access_key.access_key.access_key,
    "SQS_SECRET" : yandex_iam_service_account_static_access_key.access_key.secret_key,
    "SQS_ENDPOINT_REGION" : local.sqs_endpoint_region,
    "SEND_MESSAGE_QUEUE" : yandex_message_queue.queue["send-telegram-message-queue"].id,
  }
  content {
    zip_filename = "../zips/RainBot.UnknownMessageHandler.zip"
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

resource "yandex_function" "notification-handler" {
  name               = "notification-handler"
  user_hash          = random_string.user-hash.result
  description        = "Sends notifications to subscribers."
  runtime            = "dotnet6"
  entrypoint         = "RainBot.NotificationHandler.Handler"
  memory             = 128
  execution_timeout  = 5
  service_account_id = yandex_iam_service_account.service_account.id
  tags               = ["latest"]
  environment = {
    "YDB_DATABASE" : yandex_ydb_database_serverless.rainbot_db.database_path,
    "SQS_ACCESS_KEY" : yandex_iam_service_account_static_access_key.access_key.access_key,
    "SQS_SECRET" : yandex_iam_service_account_static_access_key.access_key.secret_key,
    "SQS_ENDPOINT_REGION" : local.sqs_endpoint_region,
    "SEND_MESSAGE_QUEUE" : yandex_message_queue.queue["send-telegram-message-queue"].id
  }
  content {
    zip_filename = "../zips/RainBot.NotificationHandler.zip"
  }
}