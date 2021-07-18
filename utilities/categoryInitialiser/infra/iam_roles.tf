resource "aws_iam_role" category_initialiser_lambda {
    name = "category_initialiser_lambda_role_${terraform.workspace}"
    assume_role_policy = data.aws_iam_policy_document.lambda_assume_role.json
    tags = local.tags
}