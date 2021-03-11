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
    profile = "jgv115"
    region = "ap-southeast-2"
  }
}

provider "aws" {
  profile = "jgv115"
  region = "ap-southeast-2"
}

provider "aws" {
  alias = "aws_us_east_1"
  profile = "jgv115"
  region = "us-east-1"
}

provider "cloudflare" {
  email   = var.CLOUDFLARE_EMAIL
  api_key = var.CLOUDFLARE_API_KEY
}