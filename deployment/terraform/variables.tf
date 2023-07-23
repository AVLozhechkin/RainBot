variable "iam_token" {
  type        = string
  sensitive   = true
  description = "Your Yandex iam token"
}

variable "telegram_bot_token" {
  type        = string
  sensitive   = true
  description = "Your Telegram bot token"
}
variable "yandex_weather_api_key" {
  type        = string
  sensitive   = true
  description = "Your Yandex Weather API key"
}

variable "folder_id" {
  type        = string
  description = "Id of your Yandex Cloud folder"
}

