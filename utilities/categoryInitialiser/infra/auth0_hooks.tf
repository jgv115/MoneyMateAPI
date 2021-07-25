resource "auth0_hook" post_user_registration {
  depends_on = [
    aws_iam_access_key.auth0_hooks,
    aws_iam_user.auth0_hooks
  ]
  name = "Post User Registration - Category Initialiser"
  script = <<EOF
module.exports = async (user, context, callback) => {

  console.log("${terraform.workspace}")
  var AWS = require("aws-sdk")

  AWS.config.update({
      accessKeyId: context.webtask.secrets.aws_client_id, secretAccessKey: context.webtask.secrets.aws_client_secret, region: "ap-southeast-2"
  })

  const result = await (new AWS.Lambda().invoke({
      FunctionName: "category_initialiser_lambda_${terraform.workspace}",
      // Payload: "test"
  }).promise());

  console.log(result)

  callback(null, { user });
}

EOF
  trigger_id = "post-user-registration"
  enabled = true

  dependencies = {
    aws-sdk = "2.950.0"
  }

  secrets {
    aws_client_id = aws_iam_access_key.auth0_hooks.id
    aws_client_secret = aws_iam_access_key.auth0_hooks.secret
  }
}

