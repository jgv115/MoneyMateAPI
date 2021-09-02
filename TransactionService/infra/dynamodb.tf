resource "aws_dynamodb_table" transaction_db {
  name = "MoneyMate_TransactionDB_${terraform.workspace}"
  hash_key = "UserIdQuery"
  range_key = "Subquery"
  billing_mode = "PAY_PER_REQUEST"

  lifecycle {
    prevent_destroy = true
  }
  
  point_in_time_recovery {
    enabled = true
  }

  attribute {
    name = "UserIdQuery"
    type = "S"
  }
  attribute {
    name = "Subquery"
    type = "S"
  }
  attribute {
    name = "TransactionTimestamp"
    type = "S"
  }
  attribute {
    name = "PayerPayeeName"
    type = "S"
  }

  local_secondary_index {
    name = "TransactionTimestampIndex"
    projection_type = "ALL"
    range_key = "TransactionTimestamp"
  }
  
  global_secondary_index {
    hash_key = "UserIdQuery"
    range_key = "PayerPayeeName"
    name = "PayerPayeeNameIndex"
    projection_type = "INCLUDE"
    non_key_attributes = ["ExternalId"]
  }

  tags = local.tags
}