data "aws_ecr_repository" "category_initialiser_image_repository" {
    name = "moneymate_category_initialiser"
}

resource "aws_lambda_function" category_initialiser_lambda {
    function_name = local.category_initialiser_lambda_name
    image_uri = "${data.aws_ecr_repository.category_initialiser_image_repository.repository_url}:${var.CATEGORY_INITIALISER_LAMBDA_IMAGE_TAG}"
    timeout = 30
    memory_size = 1024
    publish = false
    package_type = "Image"
    role = aws_iam_role.category_initialiser_lambda.arn
    
    environment {
        variables = {
        ENVIRONMENT = terraform.workspace
        }
    }
    
    tags = local.tags
}