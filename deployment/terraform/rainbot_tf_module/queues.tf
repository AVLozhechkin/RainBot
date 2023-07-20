locals {
  queue_names = toset([
    "start-handler-queue",
    "stop-handler-queue",
    "send-telegram-message-queue",
    "unknown-queue",
    "weather-queue",
    "notification-queue"
  ])
}

resource "yandex_message_queue" "queue" {
  depends_on = [yandex_resourcemanager_folder_iam_member.service_account_roles]

  for_each = local.queue_names

  name                       = each.key
  visibility_timeout_seconds = var.queue_setup.visibility_timeout_seconds
  receive_wait_time_seconds  = var.queue_setup.receive_wait_time_seconds
  max_message_size           = var.queue_setup.max_message_size
  message_retention_seconds  = var.queue_setup.message_retention_seconds
  region_id                  = var.queue_setup.region_id
  access_key                 = yandex_iam_service_account_static_access_key.access_key.access_key
  secret_key                 = yandex_iam_service_account_static_access_key.access_key.secret_key
}