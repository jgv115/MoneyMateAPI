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