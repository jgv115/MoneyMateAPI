variable MONEYMATE_API_LAMBDA_IMAGE_TAG {
  description = "Tag of image in ECR that moneymate_api_lambda will pull from"
  default = "latest"
}

variable domain_names {
  default = {
    "test": "api.test.moneymate.benong.id.au"
    "prod": "api.moneymate.benong.id.au"
  }
}

variable cloudflare_dns_entry_name {
  default = {
    "test": "api.test.moneymate"
    "prod": "api.moneymate"
  }
}

variable CLOUDFLARE_EMAIL {
  description = "Cloudflare email"
}

variable CLOUDFLARE_API_KEY {
  description = "Cloudflare master API key"
}

variable DIGITAL_OCEAN_TOKEN {
  description = "Token to provision Digital Ocean resources"
}