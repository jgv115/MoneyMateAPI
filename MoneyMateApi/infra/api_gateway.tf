# The main MoneyMate API
resource "aws_apigatewayv2_api" moneymate_api {
  name = "moneymate_api_${terraform.workspace}"
  description = "MoneyMate API"
  protocol_type = "HTTP"
}

# AWS certificate
data "aws_acm_certificate" moneymate_api_certificate {
  domain = lookup(var.domain_names, terraform.workspace, "")
}

# API Gateway custom domain name
resource "aws_apigatewayv2_domain_name" moneymate_api_custom_domain {
  domain_name = data.aws_acm_certificate.moneymate_api_certificate.domain
  domain_name_configuration {
    certificate_arn = data.aws_acm_certificate.moneymate_api_certificate.arn
    endpoint_type = "REGIONAL"
    security_policy = "TLS_1_2"
  }
}

# API Gateway custom domain name API mapping
resource "aws_apigatewayv2_api_mapping" moneymate_api_mapping {
  api_id = aws_apigatewayv2_api.moneymate_api.id
  domain_name = aws_apigatewayv2_domain_name.moneymate_api_custom_domain.domain_name
  stage = "$default"
}

# API integration with MoneyMateApi Lambda
resource "aws_apigatewayv2_integration" moneymate_api_lambda_integration {
  api_id = aws_apigatewayv2_api.moneymate_api.id
  integration_type = "AWS_PROXY"
  integration_uri = aws_lambda_function.moneymate_api_lambda.invoke_arn
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
  function_name = aws_lambda_function.moneymate_api_lambda.function_name
  principal = "apigateway.amazonaws.com"
  source_arn = "${aws_apigatewayv2_api.moneymate_api.execution_arn}/*/*"
}

resource "aws_lambda_permission" api_gateway_proxy_permission {
  action = "lambda:InvokeFunction"
  function_name = aws_lambda_function.moneymate_api_lambda.function_name
  principal = "apigateway.amazonaws.com"
  source_arn = "${aws_apigatewayv2_api.moneymate_api.execution_arn}/*/*/{proxy+}"
}