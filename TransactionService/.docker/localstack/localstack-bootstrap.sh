#!/bin/sh

echo "Creating DynamoDB table"

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
	dynamodb create-table \
	--table-name MoneyMate_TransactionDB_dev \
	--attribute-definitions AttributeName=UserIdQuery,AttributeType=S AttributeName=Subquery,AttributeType=S AttributeName=TransactionTimestamp,AttributeType=S AttributeName=PayerPayeeName,AttributeType=S\
	--key-schema AttributeName=UserIdQuery,KeyType=HASH AttributeName=Subquery,KeyType=RANGE \
	--billing-mode PAY_PER_REQUEST \
  --local-secondary-indexes \
    "[
        {
            \"IndexName\": \"TransactionTimestampIndex\",
            \"KeySchema\": [
                {\"AttributeName\": \"UserIdQuery\",\"KeyType\":\"HASH\"},
                {\"AttributeName\": \"TransactionTimestamp\",\"KeyType\":\"RANGE\"}
            ],
            \"Projection\": {
                \"ProjectionType\": \"ALL\"
            }
        }
    ]" \
  --global-secondary-indexes \
    "[
        {
            \"IndexName\": \"PayerPayeeNameIndex\",
            \"KeySchema\": [
                {\"AttributeName\":\"UserIdQuery\",\"KeyType\":\"HASH\"},
                {\"AttributeName\":\"PayerPayeeName\",\"KeyType\":\"RANGE\"}
            ],
            \"Projection\": {
                \"ProjectionType\":\"INCLUDE\",
                \"NonKeyAttributes\": [\"ExternalId\"]
            }
        }
    ]"

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|jgv115#Transaction\"},
      \"Subquery\": {\"S\": \"fa00567c-468e-4ccf-af4c-fca1c731915a\"},
      \"TransactionTimestamp\": {\"S\": \"2021-03-15T10:39:41.3123420Z\"},
      \"TransactionType\": {\"S\": \"expense\"},
      \"Amount\": {\"N\": \"123.45\"},
      \"Category\": {\"S\": \"Groceries\"},
      \"SubCategory\": {\"S\": \"Meat\"},
      \"Note\": {\"S\": \"HUGE MEATS!\"},
      \"PayerPayeeId\": {\"S\": \"fa00567c-468e-4ccf-af4c-fca1c731915a\"},
      \"PayerPayeeName\": {\"S\": \"Butcher1\"}

  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|jgv115#Transaction\"},
      \"Subquery\": {\"S\": \"fa00567c-468e-4ccf-af4c-fca1c731915b\"},
      \"TransactionTimestamp\": {\"S\": \"2021-03-14T10:39:41.3123420Z\"},
      \"TransactionType\": {\"S\": \"expense\"},
      \"Amount\": {\"N\": \"123.45\"},
      \"Category\": {\"S\": \"Groceries\"},
      \"SubCategory\": {\"S\": \"Meat\"},
      \"Note\": {\"S\": \"HUGE MEATS!\"},
      \"PayerPayeeId\": {\"S\": \"fa00567c-468e-4ccf-af4c-fca1c731915a\"},
      \"PayerPayeeName\": {\"S\": \"Butcher1\"}
  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE
  
aws --endpoint-url=http://localhost:4566 \
  --region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|jgv115#Transaction\"},
      \"Subquery\": {\"S\": \"fa00567c-468e-4ccf-af4c-fca1c731915c\"},
      \"TransactionTimestamp\": {\"S\": \"2021-03-14T10:39:41.3123420Z\"},
      \"TransactionType\": {\"S\": \"expense\"},
      \"Amount\": {\"N\": \"123.45\"},
      \"Category\": {\"S\": \"Taxes\"},
      \"SubCategory\": {\"S\": \"Fees\"},
      \"Note\": {\"S\": \"HUGE TAXES!\"},
      \"PayerPayeeId\": {\"S\": \"fa00567c-468e-4ccf-af4c-fca1c731916a\"},
      \"PayerPayeeName\": {\"S\": \"ATO\"}
  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE
  
aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|jgv115#Transaction\"},
      \"Subquery\": {\"S\": \"fa00567c-468e-4ccf-af4c-fca1c731915c\"},
      \"TransactionTimestamp\": {\"S\": \"2021-03-15T10:39:41.3123420Z\"},
      \"TransactionType\": {\"S\": \"income\"},
      \"Amount\": {\"N\": \"123.45\"},
      \"Category\": {\"S\": \"Salary\"},
      \"SubCategory\": {\"S\": \"Bank Interest\"},
      \"Note\": {\"S\": \"Westpac interest!\"}
  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|jgv115#Categories\"},
      \"Subquery\": {\"S\": \"category1\"},
      \"TransactionType\": {\"N\": \"0\"},
      \"Subcategories\": {\"SS\": [\"subcategory1\",\"subcategory2\"]}
  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|jgv115#Categories\"},
      \"Subquery\": {\"S\": \"category2\"},
      \"TransactionType\": {\"S\": \"1\"},
      \"Subcategories\": {\"SS\": [\"subcategory3\",\"subcategory4\"]}
  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|jgv115#PayersPayees\"},
      \"Subquery\": {\"S\": \"payee#a540cf4a-f21b-4cac-9e8b-168d12dcecfb\"},
      \"PayerPayeeName\": {\"S\": \"payee1\"},
      \"ExternalId\": {\"S\": \"googlePlaceId123\"}
  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|jgv115#PayersPayees\"},
      \"Subquery\": {\"S\": \"payee#a540cf4a-f21b-4cac-9e8b-168d12dcecfc\"},
      \"PayerPayeeName\": {\"S\": \"payee2\"},
      \"ExternalId\": {\"S\": \"googlePlaceId234\"}
  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|jgv115#PayersPayees\"},
      \"Subquery\": {\"S\": \"payer#9540cf4a-f21b-4cac-9e8b-168d12dcecfb\"},
      \"PayerPayeeName\": {\"S\": \"payer1\"},
      \"ExternalId\": {\"S\": \"googlePlaceId1234\"}
  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
  dynamodb put-item \
  --table-name MoneyMate_TransactionDB_dev \
  --item \
  "{
      \"UserIdQuery\": {\"S\": \"auth0|jgv115#PayersPayees\"},
      \"Subquery\": {\"S\": \"payer#9540cf4a-f21b-4cac-9e8b-168d12dcecfc\"},
      \"PayerPayeeName\": {\"S\": \"PayerTest\"},
      \"ExternalId\": {\"S\": \"googlePlaceId1235\"}
  }" \
  --return-consumed-capacity TOTAL \
  --return-item-collection-metrics SIZE


echo "Created."