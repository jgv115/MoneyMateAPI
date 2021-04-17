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