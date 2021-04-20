#!/bin/sh

aws dynamodb list-tables \
	--endpoint-url=http://127.0.0.1:4566 \
	--region ap-southeast-2 || exit 1