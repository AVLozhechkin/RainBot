resource "yandex_api_gateway" "api-gateway" {
  name        = "rain-bot-api-gateway"
  description = "API Gateway for Telegram webhooks"
  labels = {
    label       = "label"
    empty-label = ""
  }

  spec = <<-EOT
    openapi: "3.0.0"
    info:
      version: 1.0.0
      title: Rain Bot API Gateway
    paths:
      /:
        get:
          x-yc-apigateway-integration:
            type: dummy
            content:
              '*': Hello, World!
            http_code: 200
            http_headers:
              Content-Type: text/plain
      /handle-telegram-update:
        post:
          x-yc-apigateway-integration:
            type: cloud_functions
            function_id: ${yandex_function.telegram-handler.id}
            tag: "$latest"
            service_account_id: ${yandex_iam_service_account.service_account.id}
          operationId: handle-telegram-update
  EOT
}