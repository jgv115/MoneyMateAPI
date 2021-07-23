resource "auth0_hook" post_user_registration {
  name = "Post User Registration - Category Initialiser"
  script = <<EOF
module.exports = async (user, context, callback) => {

  console.log("${terraform.workspace}")
  var AWS = require("aws-sdk")

  AWS.config.update({
      accessKeyId: context.webtask.secrets.aws_client_id, secretAccessKey: context.webtask.secrets.aws_client_secret, region: "ap-southeast-2"
  })

  const result = await (new AWS.Lambda().invoke({
      FunctionName: "category_initialiser_lambda_test",
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
}

