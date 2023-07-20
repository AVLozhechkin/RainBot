variable "iam_token" {
  type        = string
  sensitive   = true
  description = "Your Yandex iam token"
}

variable "yandex_weather_api_key" {
  type        = string
  sensitive   = true
  description = "API key for Yandex Weather"
}

variable "telegram_bot_token" {
  type        = string
  sensitive   = true
  description = "Telegram bot token"
}

variable "queue_setup" {
  type = object({
    visibility_timeout_seconds = number
    receive_wait_time_seconds  = number
    max_message_size           = number
    message_retention_seconds  = number
    region_id                  = string
  })
  description = "Settings for queues"
  default = {
    visibility_timeout_seconds = 30
    receive_wait_time_seconds  = 20
    max_message_size           = 1024
    message_retention_seconds  = 345600
    region_id                  = "ru-central1"
  }
}