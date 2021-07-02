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

resource "aws_iam_policy" transaction_service_lambda_cloudwatch {
  name = "${local.transaction_service_lambda_name}_cloudwatch_acesss_policy_${terraform.workspace}"
  policy = data.aws_iam_policy_document.cloudwatch_access.json
}

resource "aws_iam_policy_attachment" transaction_service_lambda_cloudwatch {

  name = "${local.transaction_service_lambda_name}_cloudwatch_access_attachment_${terraform.workspace}"
  roles = [
    aws_iam_role.transaction_service_lambda.name]
  policy_arn = aws_iam_policy.transaction_service_lambda_cloudwatch.arn
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

resource "aws_iam_policy" transaction_service_lambda_dynamodb {
  name = "${local.transaction_service_lambda_name}_dynamodb_acesss_policy_${terraform.workspace}"
  policy = data.aws_iam_policy_document.dynamodb_access.json
}

resource "aws_iam_policy_attachment" transaction_service_lambda_dynamodb {

  name = "${local.transaction_service_lambda_name}_dynamodb_access_attachment_${terraform.workspace}"
  roles = [
    aws_iam_role.transaction_service_lambda.name]
  policy_arn = aws_iam_policy.transaction_service_lambda_dynamodb.arn
}