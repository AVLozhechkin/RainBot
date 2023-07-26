locals {
  queue_to_function = tomap({
    "subscription-handler-queue" : yandex_function.subscription-handler,
    "send-telegram-message-queue" : yandex_function.telegram-message-sender,
    "forecast-handler-queue" : yandex_function.forecast-handler
  })
}

resource "yandex_function_trigger" "triggers" {
  for_each = local.queue_to_function

  name        = "${each.value.name}-trigger"
  description = "Calls ${each.value.name} function when there is new message in ${each.key}."

  message_queue {
    queue_id           = yandex_message_queue.queue[each.key].arn
    service_account_id = yandex_iam_service_account.service_account.id
    batch_size         = "1"
    batch_cutoff       = "10"
  }

  function {
    id                 = each.value.id
    service_account_id = yandex_iam_service_account.service_account.id
  }

}

resource "yandex_function_trigger" "weather-fetcher-trigger" {
  name        = "weather-fetcher-trigger"
  description = "Fetches weather forecasts from Yandex every hour."
  timer {
    cron_expression = "0 * ? * * *"
  }
  function {
    id                 = yandex_function.yandex-weather-fetcher.id
    service_account_id = yandex_iam_service_account.service_account.id
  }
}