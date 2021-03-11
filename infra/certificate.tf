resource "aws_acm_certificate" moneymate_cert {
  domain_name = lookup(var.domain_names, terraform.workspace, "api.moneymate.benong.id.au")
  validation_method = "DNS"
  tags = local.tags
  lifecycle {
    create_before_destroy = true
  }
}

data "cloudflare_zones" benong_zones {
  filter {
    name = "benong.id.au"
  }
}

resource "cloudflare_record" moneymate_cloudflare_record {

  name = tolist(aws_acm_certificate.moneymate_cert.domain_validation_options).0["resource_record_name"]
  type = tolist(aws_acm_certificate.moneymate_cert.domain_validation_options).0["resource_record_type"]
  value = tolist(aws_acm_certificate.moneymate_cert.domain_validation_options).0["resource_record_value"]
  zone_id = data.cloudflare_zones.benong_zones.zones[0].id
}

resource "aws_acm_certificate_validation" moneymate_cert_validation {
  certificate_arn = aws_acm_certificate.moneymate_cert.arn

  validation_record_fqdns = cloudflare_record.moneymate_cloudflare_record.*.hostname
}
