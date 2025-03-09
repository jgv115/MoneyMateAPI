data "aws_iam_policy_document" lambda_assume_role {
  statement {
    actions = [
      "sts:AssumeRole"]
    principals {
      type = "Service"
      identifiers = [
        "lambda.amazonaws.com"
      ]
    }
  }
}

# Cloudwatch
data "aws_iam_policy_document" cloudwatch_access {
  statement {
    effect = "Allow"
    actions = [
      "logs:CreateLogGroup",
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]
    resources = [
      "*"]
  }
}

resource "aws_iam_policy" moneymate_api_lambda_cloudwatch {
  name = "${local.moneymate_api_lambda_name}_cloudwatch_acesss_policy_${terraform.workspace}"
  policy = data.aws_iam_policy_document.cloudwatch_access.json
}

resource "aws_iam_policy_attachment" moneymate_api_lambda_cloudwatch {

  name = "${local.moneymate_api_lambda_name}_cloudwatch_access_attachment_${terraform.workspace}"
  roles = [
    aws_iam_role.moneymate_api_lambda.name]
  policy_arn = aws_iam_policy.moneymate_api_lambda_cloudwatch.arn
}

# DynamoDB
data "aws_iam_policy_document" dynamodb_access {
  statement {
    effect = "Allow"
    actions = [
      "dynamodb:BatchGetItem",
      "dynamodb:BatchWriteItem",
      "dynamodb:Describe*",
      "dynamodb:List*",
      "dynamodb:GetItem",
      "dynamodb:Query",
      "dynamodb:Scan",
      "dynamodb:PutItem",
      "dynamodb:Update*",
      "dynamodb:DeleteItem"
    ]
    resources = [
      aws_dynamodb_table.transaction_db.arn,
      "${aws_dynamodb_table.transaction_db.arn}/index/*"
    ]
  }
}

resource "aws_iam_policy" moneymate_api_lambda_dynamodb {
  name = "${local.moneymate_api_lambda_name}_dynamodb_acesss_policy_${terraform.workspace}"
  policy = data.aws_iam_policy_document.dynamodb_access.json
}

resource "aws_iam_policy_attachment" moneymate_api_lambda_dynamodb {

  name = "${local.moneymate_api_lambda_name}_dynamodb_access_attachment_${terraform.workspace}"
  roles = [
    aws_iam_role.moneymate_api_lambda.name]
  policy_arn = aws_iam_policy.moneymate_api_lambda_dynamodb.arn
}

# Parameter Store
data "aws_ssm_parameter" google_maps_api_key {
  name = "/GooglePlaceApi/ApiKey"
}

data "aws_iam_policy_document" google_maps_api_key_access {
  statement {
    effect = "Allow"
    actions = [
      "ssm:GetParameter",
      "ssm:GetParametersByPath"
    ]
    resources = [
      "*"
    ]
  }
}

resource "aws_iam_policy" moneymate_api_lambda_parameter_store {
  name = "${local.moneymate_api_lambda_name}_parameter_store_acesss_policy_${terraform.workspace}"
  policy = data.aws_iam_policy_document.google_maps_api_key_access.json
}

resource "aws_iam_policy_attachment" moneymate_api_lambda_parameter_store {

  name = "${local.moneymate_api_lambda_name}_parameter_store_access_attachment_${terraform.workspace}"
  roles = [
    aws_iam_role.moneymate_api_lambda.name]
  policy_arn = aws_iam_policy.moneymate_api_lambda_parameter_store.arn
}

# MoneyMate API Policy + user attachment
resource "aws_iam_policy" moneymate_api_policy {
  count = terraform.workspace == "prod" ? 1 : 0
  
  name        = "moneymate_api_policy"
  description = "Policy for granting permissions for MoneyMate API"

  policy = data.aws_iam_policy_document.moneymate_api[count.index].json
}

data "aws_iam_policy_document" moneymate_api {
  count = terraform.workspace == "prod" ? 1 : 0
  
  statement {
    effect = "Allow"
    actions = [
      "ssm:GetParameter",
      "ssm:GetParametersByPath"
    ]
    resources = [
      "*"
    ]
  }
}

resource "aws_iam_user_policy_attachment" "user_policy_attach" {
  count = terraform.workspace == "prod" ? 1 : 0
  
  user       = aws_iam_user.moneymate_api_user[count.index].name
  policy_arn = aws_iam_policy.moneymate_api_policy[count.index].arn
}