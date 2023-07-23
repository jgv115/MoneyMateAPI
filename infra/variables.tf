variable domain_names {
  default = {
    "test": "api.test.moneymate.benong.id.au"
    "prod": "api.moneymate.benong.id.au"
  }
}

variable CLOUDFLARE_EMAIL {
  description = "Cloudflare email"
}

variable CLOUDFLARE_API_KEY {
  description = "Cloudflare master API key"
}

variable cockroach_db_cluster_name {
  default = "moneymate-cluster"
}