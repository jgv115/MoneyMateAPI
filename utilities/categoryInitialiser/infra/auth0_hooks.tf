resource "auth0_hook" post_user_registration {
  name = "Post User Registration - Category Initialiser"
  script = <<EOF
function (user, context, callback) {
  console.log(context)
  console.log(${terraform.workspace})
  console.log(context.webtask.secrets.test_secret)
  
  callback(null, { user });
}
EOF
  trigger_id = "post-user-registration"
  enabled = true

  dependencies = {
    aws-sdk = "^2.949.0"
  }
}