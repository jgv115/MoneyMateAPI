resource "aws_dynamodb_table" transaction_db {
  name = "MoneyMate_TransactionDB_${terraform.workspace}"
  hash_key = "UserId"
  range_key = "Date"
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
    name = "TransactionId"
    type = "S"
  }

  local_secondary_index {
    name = "TransactionIdIndex"
    projection_type = "ALL"
    range_key = "TransactionId"
  }

  tags = local.tags
}