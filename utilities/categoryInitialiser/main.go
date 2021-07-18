package main

import (
	"categoryInitialiser/models"
	"fmt"
	"github.com/aws/aws-lambda-go/lambda"

)

func handle(request models.IntialiseCategoriesRequest) {
	fmt.Println("hello")
}

func main() {
	lambda.Start(handle)
}