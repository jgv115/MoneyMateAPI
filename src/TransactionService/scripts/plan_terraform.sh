#!/usr/bin/env sh

export TF_WORKSPACE=test

terraform init
#terraform workspace select test
terraform plan