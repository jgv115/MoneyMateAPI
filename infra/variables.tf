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

variable cockroach_db_cluster_id {
  description = "Hard coded cluster Id from Cockroach DB console"
  default = "c879b140-a509-49fd-ac61-ee5a5c381026"
}