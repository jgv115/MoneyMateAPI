terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "~> 5.0"
    }

    digitalocean = {
      source = "digitalocean/digitalocean"
      version = "2.49.1"
    }
    
    cloudflare = {
      source = "cloudflare/cloudflare"
      version = "5.1.0"
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

provider "cloudflare" {
  email   = var.CLOUDFLARE_EMAIL
  api_key = var.CLOUDFLARE_API_KEY
}

# Configure the DigitalOcean Provider
provider "digitalocean" {
  token = var.DIGITAL_OCEAN_TOKEN
}