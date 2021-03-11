terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "~> 3.0"
    }
    cloudflare = {
      source = "cloudflare/cloudflare"
      version = "~> 2.0"
    }
  }

  backend "s3" {
    bucket = "moneymate-api-core-infra"
    key = "moneymate-api-core-infra.tfstate"
//    profile = "jgv115"
    region = "ap-southeast-2"
  }
}

provider "aws" {
//  profile = "jgv115"
  region = "ap-southeast-2"
}

provider "cloudflare" {
  email   = "jgv115@gmail.com"
  api_key = "30cd54bc0b695b3e519a218b93488b06748d8"
}