terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 3.0"
    }
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "2.49.1"
    }
    cloudflare = {
      source  = "cloudflare/cloudflare"
      version = "~> 2.0"
    }
    cockroach = {
      source  = "cockroachdb/cockroach"
      version = "0.7.0"
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
  email   = var.CLOUDFLARE_EMAIL
  api_key = var.CLOUDFLARE_API_KEY
}

provider "cockroach" {
}

# Configure the DigitalOcean Provider
provider "digitalocean" {
  token = var.DIGITAL_OCEAN_TOKEN
}