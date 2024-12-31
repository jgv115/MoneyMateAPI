// This is created manually
data "aws_ecr_repository" moneymate_api_image_repository {
  name = "moneymate/moneymate_api"
}


resource "aws_lambda_function" moneymate_api_lambda {
  function_name = local.moneymate_api_lambda_name
  image_uri = "${data.aws_ecr_repository.moneymate_api_image_repository.repository_url}:${var.MONEYMATE_API_LAMBDA_IMAGE_TAG}"
  image_config {
    command = ["MoneyMateApi::MoneyMateApi.LambdaEntryPoint::FunctionHandlerAsync"]
    entry_point = ["/lambda-entrypoint.sh"]
  }
  timeout = 30
  memory_size = 2048
  publish = false
  package_type = "Image"
  role = aws_iam_role.moneymate_api_lambda.arn
  
  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT = terraform.workspace
    }
  }
  
  tags = local.tags
}