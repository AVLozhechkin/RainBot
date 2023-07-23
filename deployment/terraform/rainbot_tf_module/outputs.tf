output "api_gateway_webhook_endpoint" {
  value = "${yandex_api_gateway.api-gateway.domain}/handle-telegram-update"
}

output "yandex_database_endpoint" {
  value = yandex_ydb_database_serverless.rainbot_db.ydb_api_endpoint
}

output "yandex_database_path" {
  value = yandex_ydb_database_serverless.rainbot_db.database_path
}