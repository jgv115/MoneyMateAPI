#!/bin/sh

aws --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
	dynamodb create-table \
	--table-name MoneyMate_TransactionDB_dev \
	--attribute-definitions AttributeName=UserIdQuery,AttributeType=S AttributeName=Subquery,AttributeType=S AttributeName=TransactionTimestamp,AttributeType=S\
	--key-schema AttributeName=UserIdQuery,KeyType=HASH AttributeName=Subquery,KeyType=RANGE \
	--provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
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
    ]"


zip -j categoryInitialiser.zip /code/bin/main

aws lambda create-function \
    --endpoint-url=http://localhost:4566 \
	--region ap-southeast-2 \
    --function-name category_initialiser_lambda \
    --role lambda-role \
    --handler main \
    --runtime go1.x \
    --zip-file fileb://categoryInitialiser.zip \
    --environment Variables=\{LOCALSTACK_HOSTNAME=localstack,ENVIRONMENT=dev\}
