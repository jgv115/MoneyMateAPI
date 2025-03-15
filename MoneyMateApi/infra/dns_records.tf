# Cloudflare DNS record for custom domain name
data "cloudflare_zones" benong_zones {
    name = "benong.id.au"
}

// Point test environment to AWS Lambda
resource "cloudflare_dns_record" moneymate_cloudflare_record_test {
  count = terraform.workspace == "test" ? 1 : 0
  
  zone_id = data.cloudflare_zones.benong_zones.result.0.id
  name = lookup(var.cloudflare_dns_entry_name, terraform.workspace, "")
  
  content = aws_apigatewayv2_domain_name.moneymate_api_custom_domain.domain_name_configuration.0.target_domain_name
  type = "CNAME"
  ttl = 3600
  proxied = false
}

// Point prod environment to Digital Ocean
resource "cloudflare_dns_record" "moneymate_cloudflare_record_prod" {
  count = terraform.workspace == "prod" ? 1 : 0

  zone_id = data.cloudflare_zones.benong_zones.result.0.id
  name = lookup(var.cloudflare_dns_entry_name, terraform.workspace, "")
  content = digitalocean_app.moneymate_api[count.index].live_domain
  type = "CNAME"
  ttl = 3600
  proxied = false
}