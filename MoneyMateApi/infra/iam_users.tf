# Create AWS IAM user for SSM access
resource "aws_iam_user" "moneymate_api_user" {
  count = terraform.workspace == "prod" ? 1 : 0
  
  name = "moneymate-api-user"
  path = "/service-accounts/"
}

# Create access keys for the IAM user
resource "aws_iam_access_key" "moneymate_api_user_key" {
  count = terraform.workspace == "prod" ? 1 : 0
  
  user = aws_iam_user.moneymate_api_user[count.index].name
}
