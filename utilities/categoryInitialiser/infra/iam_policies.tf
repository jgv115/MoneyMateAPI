data "aws_iam_policy_document" "lambda_assume_role" {
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
data "aws_iam_policy_document" "cloudwatch_access" {
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

resource "aws_iam_policy" "category_initialiser_lambda_cloudwatch" {
  name   = "${local.category_initialiser_lambda_name}_cloudwatch_acesss_policy_${terraform.workspace}"
  policy = data.aws_iam_policy_document.cloudwatch_access.json
}

resource "aws_iam_policy_attachment" "category_initialiser_lambda_cloudwatch" {

  name = "${local.category_initialiser_lambda_name}_cloudwatch_access_attachment_${terraform.workspace}"
  roles = [
  aws_iam_role.category_initialiser_lambda.name]
  policy_arn = aws_iam_policy.category_initialiser_lambda_cloudwatch.arn
}

# SSM
data "aws_ssm_parameter" "cockroach_db_connection_string" {
  name = "/${terraform.workspace}/categoryInitialiser/cockroachDbConnectionString"
}

data "aws_iam_policy_document" "ssm_access" {
  statement {
    effect = "Allow"
    actions = [
      "ssm:GetParameters",
      "ssm:GetParameter",
    ]
    resources = [
    data.aws_ssm_parameter.cockroach_db_connection_string.arn]
  }
}

resource "aws_iam_policy" "category_initialiser_lambda_ssm" {
  name   = "${local.category_initialiser_lambda_name}_ssm_acesss_policy_${terraform.workspace}"
  policy = data.aws_iam_policy_document.ssm_access.json
}

resource "aws_iam_policy_attachment" "category_initialiser_lambda_ssm" {

  name = "${local.category_initialiser_lambda_name}_ssm_access_attachment_${terraform.workspace}"
  roles = [
  aws_iam_role.category_initialiser_lambda.name]
  policy_arn = aws_iam_policy.category_initialiser_lambda_ssm.arn
}

# DynamoDB
data "aws_dynamodb_table" "transaction_db" {
  name = "MoneyMate_TransactionDB_${terraform.workspace}"
}

data "aws_iam_policy_document" "dynamodb_access" {
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
      data.aws_dynamodb_table.transaction_db.arn,
      "${data.aws_dynamodb_table.transaction_db.arn}/index/*"
    ]
  }
}

resource "aws_iam_policy" "category_initialiser_lambda_dynamodb" {
  name   = "${local.category_initialiser_lambda_name}_dynamodb_acesss_policy_${terraform.workspace}"
  policy = data.aws_iam_policy_document.dynamodb_access.json
}

resource "aws_iam_policy_attachment" "category_initialiser_lambda_dynamodb" {

  name = "${local.category_initialiser_lambda_name}_dynamodb_access_attachment_${terraform.workspace}"
  roles = [
  aws_iam_role.category_initialiser_lambda.name]
  policy_arn = aws_iam_policy.category_initialiser_lambda_dynamodb.arn
}
