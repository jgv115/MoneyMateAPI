variable "auth0_management_api_client_id" {
  description = "The Client ID of the Auth0 management API"
  type = string
}

variable "auth0_management_api_client_secret" {
  description = "The Client Secret of the Auth0 management API"
  type = string
  default = "Smn0SGJxuV34wbcZ03ChKxcgJKjeTYwb"
}

variable "auth0_domain" {
  description = "Auth0 Domain for tenant"
}

variable "auth0_admin_user_password" {
  description = "Password for the admin user to be created for Auth0 Tenant"
}

variable "moneymate_api_identifiers" {
  default = {
    "dev": "https://api.dev.moneymate.benong.id.au"
    "test": "https://api.test.moneymate.benong.id.au"
    "prod": "https://api.moneymate.benong.id.au"
  }
}

variable moneymate_web_domain {
  default = {
    "dev": "http://localhost:3000",
    "test": "https://test.moneymate.benong.id.au",
    "prod": "https://moneymate.benong.id.au"
  }
}