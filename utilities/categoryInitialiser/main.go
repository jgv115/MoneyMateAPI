package main

import (
	"categoryInitialiser/category_provider"
	"categoryInitialiser/handler"
	"categoryInitialiser/models"
	"categoryInitialiser/store"
	_ "embed"
	"errors"
	"fmt"
	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"os"
)

func handle(request models.IntialiseCategoriesRequest) (err error) {
	categoryProvider, categoriesRepository, err := setupDependencies()

	err = handler.HandleRequest(request.Id, categoryProvider, categoriesRepository)
	if err != nil {
		return err
	}

	return
}

func createAWSSession() (sess *session.Session) {
	localstackHostname, localstackFound := os.LookupEnv("LOCALSTACK_HOSTNAME")

	fmt.Printf(">>> %+v\n", localstackFound)
	if localstackFound {
		sess = session.Must(session.NewSession(&aws.Config{
			Endpoint: aws.String(fmt.Sprintf("http://%s:4566", localstackHostname)),
			Region:   aws.String("ap-southeast-2"),
		}))
	} else {
		sess = session.Must(session.NewSessionWithOptions(session.Options{
			SharedConfigState: session.SharedConfigDisable,
		}))
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

	awsSession := createAWSSession()
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
