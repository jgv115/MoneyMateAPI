resource auth0_user admin_user {
  connection_name = auth0_connection.username_password.name
  user_id = "jgv115"
  given_name = "Benjamin"
  family_name = "Ong"
  nickname = "Ben"
  email = "jgv115@gmail.com"
  email_verified = true
  password = var.auth0_admin_user_password
}

resource auth0_user integration_test_user {
  count = terraform.workspace == "dev" ? 1 : 0
  connection_name = auth0_connection.username_password.name
  user_id = "moneymatetest"
  given_name = "MoneyMate"
  family_name = "Test"
  nickname = "MoneyTest"
  email = "test@moneymate.com"
  email_verified = true
  password = "test123"
}