variable CATEGORY_INITIALISER_LAMBDA_IMAGE_TAG {
  description = "Tag of image in ECR that category_initialiser_lambda will pull from"
  default = "latest"
}

variable "auth0_management_api_client_id" {
  description = "The Client ID of the Auth0 management API"
  type = string
}

variable "auth0_management_api_client_secret" {
  description = "The Client Secret of the Auth0 management API"
  type = string
  default = "Smn0SGJxuV34wbcZ03ChKxcgJKjeTYwb"
}

variable "auth0_domain" {
  description = "Auth0 Domain for tenant"
}