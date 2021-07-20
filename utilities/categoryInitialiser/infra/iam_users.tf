resource "aws_iam_user" auth0_hooks {
  name = "auth0_hooks_user_${terraform.workspace}"

  tags = local.tags
}

resource "aws_iam_access_key" auth0_hooks {
  user = aws_iam_user.auth0_hooks.name
}

data aws_iam_policy_document auth0_hooks {
  statement {
    effect = "Allow"
    actions = [
      "lambda:InvokeFunction"
    ]
    resources = [
      aws_lambda_function.category_initialiser_lambda.arn
    ]
  }
}

resource aws_iam_user_policy auth0_hooks {
  policy = data.aws_iam_policy_document.auth0_hooks.json
  user = aws_iam_user.auth0_hooks.name
}

resource "aws_ssm_parameter" auth0_hooks_access_id {
  name = "auth0_hooks_user_access_id_${terraform.workspace}"
  type = "SecureString"
  value = aws_iam_access_key.auth0_hooks.id
  tags = local.tags
}

resource "aws_ssm_parameter" auth0_hooks_access_secret {
  name = "auth0_hooks_user_access_secret_${terraform.workspace}"
  type = "SecureString"
  value = aws_iam_access_key.auth0_hooks.secret
  tags = local.tags
}