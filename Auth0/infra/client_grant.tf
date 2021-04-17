resource "auth0_resource_server" moneymate_api {
  allow_offline_access = true
  enforce_policies = false
  identifier = lookup(var.moneymate_api_identifiers, terraform.workspace)
  name = "MoneyMate API"
  signing_alg = "RS256"
  skip_consent_for_verifiable_first_party_clients = true
  token_dialect = "access_token"
  token_lifetime = 86400
  token_lifetime_for_web = 7200
}

resource "auth0_client_grant" "moneymate_api" {
  client_id = auth0_client.moneymate_app.id
  audience = auth0_resource_server.moneymate_api.identifier
  scope = []
}