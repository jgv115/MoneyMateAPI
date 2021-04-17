resource "auth0_connection" username_password {
  name = "Username-Password-Authentication"
  strategy = "auth0"
  is_domain_connection = false
  enabled_clients = [auth0_client.moneymate_app.id, auth0_client.debug_app.id, var.auth0_management_api_client_id]
}

resource "auth0_connection" google {
  name = "google-oauth2"
  options {
    scopes = ["email", "profile"]
  }
  strategy = "google-oauth2"
  is_domain_connection = false
  enabled_clients = [auth0_client.moneymate_app.id]
}