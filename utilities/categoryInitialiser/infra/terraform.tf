terraform {
  required_providers {
    auth0 = {
      source = "alexkappa/auth0"
      version = "0.20.0"
    }
    aws = {
      source = "hashicorp/aws"
      version = "~> 3.0"
    }
  }
  backend "s3" {
    bucket = "moneymate-api-category-initialiser-infra"
    key = "moneymate-api-category-initialiser-infra.tfstate"
//    profile = "jgv115"
    region = "ap-southeast-2"
  }
}

provider "aws" {
//  profile = "jgv115"
  region = "ap-southeast-2"
}

provider "auth0" {
  client_id = var.auth0_management_api_client_id
  client_secret = var.auth0_management_api_client_secret
  domain = var.auth0_domain
  debug = true
}