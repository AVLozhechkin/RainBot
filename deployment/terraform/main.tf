terraform {
  required_providers {
    yandex = {
      source = "yandex-cloud/yandex"
    }
  }
  required_version = ">= 0.13"
}

provider "yandex" {
  token     = var.iam_token
  folder_id = var.folder_id
  zone      = "ru-central1-a"
}

variable "iam_token" {
  type        = string
  sensitive   = true
  description = "Your Yandex iam token"
}

variable "folder_id" {
  type        = string
  description = "Id of your Yandex Cloud folder"
}


module "rainbot" {
  source = "./rainbot_tf_module"

  iam_token              = var.iam_token
  telegram_bot_token     = "test string"
  yandex_weather_api_key = "test key"
}