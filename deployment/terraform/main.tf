terraform {
  backend "s3" {
    bucket = "moscow-rainbot-terraform-state-bucket"
    region = "ru-central1"
    key = "terraform.tfstate"
    endpoint = "https://storage.yandexcloud.net"

    
    skip_region_validation = true
    skip_credentials_validation = true
  }
  required_providers {
    yandex = {
      source = "yandex-cloud/yandex"
    }
  }
  required_version = ">= 0.13"
}

provider "yandex" {
  token     = var.iam_token
  zone      = "ru-central1-a"
  folder_id = var.folder_id
}

resource "null_resource" "prepare_code" {

  triggers = {
    id = timestamp()
  }
  provisioner "local-exec" {
    command = "python ../prepare_code.py"
  }
}

module "rainbot" {
  depends_on = [null_resource.prepare_code]

  source = "./rainbot_tf_module"

  iam_token              = var.iam_token
  telegram_bot_token     = var.telegram_bot_token
  yandex_weather_api_key = var.yandex_weather_api_key
  longitude = var.longitude
  latitude = var.latitude
}

resource "null_resource" "set-webhooks" {
  provisioner "local-exec" {
    command = "curl -F url=https://${module.rainbot.api_gateway_webhook_endpoint} https://api.telegram.org/bot${var.telegram_bot_token}/setWebhook"
  }
}

resource "null_resource" "create-database-tables" {
  provisioner "local-exec" {
    command = "python ../create_tables.py -d ${module.rainbot.yandex_database_path} -t ${var.iam_token} -e ${module.rainbot.yandex_database_endpoint}"
  }
}

resource "null_resource" "delete-zips" {
  depends_on = [module.rainbot]

  triggers = {
    id = timestamp()
  }

  provisioner "local-exec" {
    command = "rm -r ../zips"
  }
}