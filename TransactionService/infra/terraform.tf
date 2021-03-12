terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "~> 3.0"
    }
  }
  backend "s3" {
    bucket = "moneymate-api-transaction-service-infra"
    key = "moneymate-api-transaction-service-infra.tfstate"
//    profile = "jgv115"
    region = "ap-southeast-2"
  }
}

provider "aws" {
//  profile = "jgv115"
  region = "ap-southeast-2"
}