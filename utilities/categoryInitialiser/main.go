package main

import (
	"categoryInitialiser/aws_utils"
	"categoryInitialiser/category_provider"
	"categoryInitialiser/models"
	"categoryInitialiser/request_handler"
	"categoryInitialiser/store"
	_ "embed"
	"errors"
	"fmt"
	"os"

	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/service/dynamodb"
)

func handle(request models.IntialiseCategoriesRequest) (err error) {
	categoryProvider, categoriesRepository, err := setupDependencies()

	err = request_handler.HandleRequest(request.Id, categoryProvider, categoriesRepository)
	if err != nil {
		return err
	}

	return
}

//go:embed category_provider/data/expenseCategories.json
var expenseCategories []byte

//go:embed category_provider/data/incomeCategories.json
var incomeCategories []byte

func setupDependencies() (categoryProvider category_provider.CategoryProvider, categoriesRepository store.CategoriesRepository, err error) {
	environment, ok := os.LookupEnv("ENVIRONMENT")
	if !ok {
		err = errors.New("no ENVIRONMENT environment variable found")
		return
	}

	awsSession := aws_utils.CreateAWSSession(environment)
	dynamoDbClient := dynamodb.New(awsSession)

	categoriesRepository = &store.DynamoDbCategoriesRepository{
		DynamoDbClient: dynamoDbClient,
		TableName:      fmt.Sprintf("MoneyMate_TransactionDB_%v", environment),
	}

	categoryProvider = &category_provider.JsonCategoryProvider{
		CategoryJsonBytes: map[string][]byte{
			"expense": expenseCategories,
			"income":  incomeCategories,
		},
	}

	return
}

func main() {
	lambda.Start(handle)
}
