terraform {
  required_providers {
    auth0 = {
      source = "alexkappa/auth0"
      version = "0.20.0"
    }
  }
  backend "s3" {
    bucket = "moneymate-api-auth0-infra"
    key = "moneymate-api-auth0-infra.tfstate"
//    profile = "jgv115"
    region = "ap-southeast-2"
  }
}

provider "auth0" {
  client_id = var.auth0_management_api_client_id
  client_secret = var.auth0_management_api_client_secret
  domain = var.auth0_domain
  debug = true
}