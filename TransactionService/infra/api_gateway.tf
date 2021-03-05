# The main MoneyMate API
resource "aws_apigatewayv2_api" moneymate_api {
  name = "moneymate_api_${terraform.workspace}"
  description = "MoneyMate API"
  protocol_type = "HTTP"
}

# API integration with MoneyMate TransactionService Lambda
resource "aws_apigatewayv2_integration" moneymate_api_lambda_integration {
  api_id = aws_apigatewayv2_api.moneymate_api.id
  integration_type = "AWS_PROXY"
  integration_uri = aws_lambda_function.transaction_service_lambda.invoke_arn
  payload_format_version = "2.0"
}

# API Routes
resource "aws_apigatewayv2_route" moneymate_api_proxy_route {
  depends_on = [aws_apigatewayv2_integration.moneymate_api_lambda_integration]
  api_id = aws_apigatewayv2_api.moneymate_api.id
  route_key = "ANY /{proxy+}"
  target = "integrations/${aws_apigatewayv2_integration.moneymate_api_lambda_integration.id}"
}
resource "aws_apigatewayv2_route" moneymate_api_root_route {
  depends_on = [aws_apigatewayv2_integration.moneymate_api_lambda_integration]
  api_id = aws_apigatewayv2_api.moneymate_api.id
  route_key = "ANY /"
  target = "integrations/${aws_apigatewayv2_integration.moneymate_api_lambda_integration.id}"
}

resource "aws_apigatewayv2_stage" moneymate_api {
  api_id = aws_apigatewayv2_api.moneymate_api.id

  name = "$default" 
  auto_deploy = true
}

resource "aws_apigatewayv2_deployment" moneymate_api {
  depends_on = [
  aws_apigatewayv2_integration.moneymate_api_lambda_integration,
  aws_apigatewayv2_route.moneymate_api_proxy_route]
  
  api_id = aws_apigatewayv2_api.moneymate_api.id
}

# Lambda invoke permissions
resource "aws_lambda_permission" api_gateway_root_permission {
  action = "lambda:InvokeFunction"
  function_name = aws_lambda_function.transaction_service_lambda.function_name
  principal = "apigateway.amazonaws.com"
  source_arn = "${aws_apigatewayv2_api.moneymate_api.execution_arn}/*/*"
}

resource "aws_lambda_permission" api_gateway_proxy_permission {
  action = "lambda:InvokeFunction"
  function_name = aws_lambda_function.transaction_service_lambda.function_name
  principal = "apigateway.amazonaws.com"
  source_arn = "${aws_apigatewayv2_api.moneymate_api.execution_arn}/*/*/{proxy+}"
}