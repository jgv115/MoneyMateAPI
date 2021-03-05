locals {
  tags = {
    environment = terraform.workspace
  }
  transaction_service_lambda_name = "transaction_service_lambda_${terraform.workspace}"
}