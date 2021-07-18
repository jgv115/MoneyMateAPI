data "aws_ecr_repository" "category_initialiser_image_repository" {
    name = "moneymate_category_initialiser"
}

resource "aws_ecr_repository_policy" ecr_policy {
    repository = data.aws_ecr_repository.category_initialiser_image_repository.name
    policy = <<EOF
{
  "rules": [
    {
      "rulePriority": 1,
      "description": "Keep only 2 images in ECR",
      "selection": {
        "tagStatus": "any",
        "countType": "imageCountMoreThan",
        "countNumber": 2
      },
      "action": {
        "type": "expire"
      }
    }
  ]
}
EOF
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