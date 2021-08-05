#!/bin/sh

aws dynamodb describe-table \
	--endpoint-url=http://127.0.0.1:4566 \
	--region ap-southeast-2 \
	--table-name MoneyMate_TransactionDB_dev || exit 1