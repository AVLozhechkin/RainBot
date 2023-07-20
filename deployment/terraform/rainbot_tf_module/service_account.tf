resource "yandex_iam_service_account" "service_account" {
  name        = "rainbot-service-account"
  description = "Service account for Terraform"
}

resource "yandex_resourcemanager_folder_iam_member" "service_account_roles" {
  depends_on = [yandex_iam_service_account_static_access_key.access_key]

  for_each = toset([
    "ymq.admin",                          # Required to create and delete queues, read/write messages
    "ydb.editor",                         # Required to create db instance
    "iam.serviceAccounts.user",           # Required for using service accounts 
    "iam.serviceAccounts.accessKeyAdmin", # Required to create access keys
    # "functions.functionInvoker", # 
    "functions.editor" # Required to create and delete functons
  ])
  role = each.key

  folder_id = yandex_iam_service_account.service_account.folder_id

  member = "serviceAccount:${yandex_iam_service_account.service_account.id}"
}

resource "yandex_iam_service_account_static_access_key" "access_key" {
  service_account_id = yandex_iam_service_account.service_account.id
}