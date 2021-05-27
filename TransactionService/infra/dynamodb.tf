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

  local_secondary_index {
    name = "TransactionTimestampIndex"
    projection_type = "ALL"
    range_key = "TransactionTimestamp"
  }

  tags = local.tags
}