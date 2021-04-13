#!/bin/sh

echo "Creating DynamoDB table"

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
	dynamodb create-table \
	--table-name MoneyMate_TransactionDB_dev \
	--attribute-definitions AttributeName=UserIdQuery,AttributeType=S AttributeName=Subquery,AttributeType=S AttributeName=TransactionId,AttributeType=S\
	--key-schema AttributeName=UserIdQuery,KeyType=HASH AttributeName=Subquery,KeyType=RANGE \
	--provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
  --local-secondary-indexes \
    "[
        {
            \"IndexName\": \"TransactionIdIndex\",
            \"KeySchema\": [
                {\"AttributeName\": \"UserIdQuery\",\"KeyType\":\"HASH\"},
                {\"AttributeName\": \"TransactionId\",\"KeyType\":\"RANGE\"}
            ],
            \"Projection\": {
                \"ProjectionType\": \"ALL\"
            }
        }
    ]"

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|60474554677179006923e9a6#Transaction\"},
      \"Subquery\": {\"S\": \"2021-03-15T10:39:41.3123420Z\"},
      \"TransactionId\": {\"S\": \"fa00567c-468e-4ccf-af4c-fca1c731915a\"},
      \"TransactionType\": {\"S\": \"expense\"},
      \"Amount\": {\"N\": \"123.45\"},
      \"Category\": {\"S\": \"Groceries\"},
      \"SubCategory\": {\"S\": \"Meat\"}
  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE
  
aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|60474554677179006923e9a6#Transaction\"},
      \"Subquery\": {\"S\": \"2021-03-14T10:39:41.3123420Z\"},
      \"TransactionId\": {\"S\": \"fa00567c-468e-4ccf-af4c-fca1c731915a\"},
      \"TransactionType\": {\"S\": \"expense\"},
      \"Amount\": {\"N\": \"123.45\"},
      \"Category\": {\"S\": \"Groceries\"},
      \"SubCategory\": {\"S\": \"Meat\"}
  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE
  
echo "Created."