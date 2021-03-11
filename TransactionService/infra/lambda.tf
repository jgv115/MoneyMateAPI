data "aws_ecr_repository" transaction_service_image_repository {
  name = "moneymate_transaction_service"
}

resource "aws_lambda_function" transaction_service_lambda {
  function_name = local.transaction_service_lambda_name
  image_uri = "${data.aws_ecr_repository.transaction_service_image_repository.repository_url}:${var.TRANSACTION_SERVICE_LAMBDA_IMAGE_TAG}"
  image_config {
    command = ["TransactionService::TransactionService.LambdaEntryPoint::FunctionHandlerAsync"]
    entry_point = ["/lambda-entrypoint.sh"]
  }
  timeout = 30
  memory_size = 1024
  publish = false
  package_type = "Image"
  role = aws_iam_role.transaction_service_lambda.arn
  
  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT = terraform.workspace
    }
  }
  
  tags = local.tags
}