resource "aws_iam_role" transaction_service_lambda {
  name = "transaction_service_lambda_role_${terraform.workspace}"
  assume_role_policy = data.aws_iam_policy_document.lambda_assume_role.json
  tags = local.tags
}