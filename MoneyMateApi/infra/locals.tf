locals {
  tags = {
    environment = terraform.workspace
  }
  moneymate_api_lambda_name = "moneymate_api_lambda_${terraform.workspace}"
}