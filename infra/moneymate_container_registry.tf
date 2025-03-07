resource digitalocean_container_registry moneymate_registry {
  count = terraform.workspace == "prod" ? 1 : 0
  
  name = "moneymate-api"
  subscription_tier_slug = "starter"
}