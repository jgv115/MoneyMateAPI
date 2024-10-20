resource "auth0_client" moneymate_app {
  name = "MoneyMate App"
  app_type = "native"
  callbacks = ["moneymateapp://${var.auth0_domain}/android/com.jgv115.moneymateapp/callback", "com.jgv115.moneymateapp://${var.auth0_domain}/ios/com.jgv115.moneymateapp/callback"]
  allowed_logout_urls = ["moneymateapp://${var.auth0_domain}/android/com.jgv115.moneymateapp/callback", "com.jgv115.moneymateapp://${var.auth0_domain}/ios/com.jgv115.moneymateapp/callback"]
  cross_origin_auth = false
  custom_login_page_on = true
  grant_types = ["authorization_code", "implicit", "refresh_token"]
  is_first_party = true
  is_token_endpoint_ip_header_trusted = false
  oidc_conformant = true
  sso_disabled = false
  token_endpoint_auth_method = "none"
  jwt_configuration {
    alg = "RS256"
    lifetime_in_seconds = 36000
    secret_encoded = false
  }
  refresh_token {
    rotation_type = "rotating"
    expiration_type = "expiring"
    leeway = 0
    token_lifetime = 2592000
    infinite_idle_token_lifetime = true
    infinite_token_lifetime      = false
    idle_token_lifetime          = 1296000
  }
}

resource "auth0_client" moneymate_web {
  name = "MoneyMate Web"
  app_type = "regular_web"
  callbacks = ["${lookup(var.moneymate_web_domain, terraform.workspace)}"]
  allowed_logout_urls                 = ["${lookup(var.moneymate_web_domain, terraform.workspace)}"]
  web_origins                         = ["${lookup(var.moneymate_web_domain, terraform.workspace)}"]
  cross_origin_auth = false
  custom_login_page_on = true
  grant_types = ["authorization_code", "implicit", "refresh_token"]
  is_first_party = true
  is_token_endpoint_ip_header_trusted = false
  oidc_conformant = true
  sso_disabled = false
  token_endpoint_auth_method = "none"
  jwt_configuration {
    alg = "RS256"
    lifetime_in_seconds = 36000
    secret_encoded = false
  }
  refresh_token {
    rotation_type = "rotating"
    expiration_type = "expiring"
    leeway = 0
    token_lifetime = 2592000
    infinite_idle_token_lifetime = true
    infinite_token_lifetime      = false
    idle_token_lifetime          = 1296000
  }
}

resource "auth0_client" debug_app {
  name = "Debug App"
  app_type = "regular_web"
  callbacks = ["https://eni50qndu1cg3w7.m.pipedream.net"]
  cross_origin_auth = false
  custom_login_page_on = true
  grant_types = ["authorization_code", "implicit", "refresh_token", "password"]
  is_first_party = true
  is_token_endpoint_ip_header_trusted = false
  oidc_conformant = true
  sso_disabled = false
  token_endpoint_auth_method = "none"
  jwt_configuration {
    alg = "RS256"
    lifetime_in_seconds = 36000
    secret_encoded = false
  }
  refresh_token {
    rotation_type = "rotating"
    expiration_type = "expiring"
    leeway = 0
    token_lifetime = 2592000
    infinite_idle_token_lifetime = true
    infinite_token_lifetime      = false
    idle_token_lifetime          = 1296000
  }
}