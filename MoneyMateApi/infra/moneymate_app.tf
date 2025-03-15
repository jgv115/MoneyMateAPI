# Create DigitalOcean App

data "digitalocean_container_registry" moneymate_api_registry {
  name  = "moneymate-api"
  count = terraform.workspace == "prod" ? 1 : 0
}

resource "digitalocean_app" "moneymate_api" {
  count = terraform.workspace == "prod" ? 1 : 0

  spec {
    name   = "moneymate-app"
    region = "syd1"

    service {
      name = "moneymate-api"
      # environment_slug   = "dotnet"
      instance_count     = 1
      instance_size_slug = "basic-xxs"

      # Reference to container registry
      image {
        registry_type = "DOCR"
        repository    = data.digitalocean_container_registry.moneymate_api_registry[count.index].name
        tag           = var.MONEYMATE_API_LAMBDA_IMAGE_TAG
        deploy_on_push {
          enabled = true
        }
      }

      # AWS credentials environment variables
      env {
        key   = "AWS_ACCESS_KEY_ID"
        value = aws_iam_access_key.moneymate_api_user_key[count.index].id
        type  = "SECRET"
      }

      env {
        key   = "AWS_SECRET_ACCESS_KEY"
        value = aws_iam_access_key.moneymate_api_user_key[count.index].secret
        type  = "SECRET"
      }

      env {
        key   = "AWS_REGION"
        value = "ap-southeast-2"
      }

      # Application environment variables
      env {
        key   = "ASPNETCORE_ENVIRONMENT"
        value = terraform.workspace
        type  = "GENERAL"
      }
    }
  }
}