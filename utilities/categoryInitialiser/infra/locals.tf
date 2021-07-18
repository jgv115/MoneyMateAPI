locals {
  tags = {
    environment = terraform.workspace
  }
  category_initialiser_lambda_name = "category_initialiser_lambda_${terraform.workspace}"
}