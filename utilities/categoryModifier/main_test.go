package main

import (
	"categoryModifier/awsConfig"
	"categoryModifier/models"
	"context"
	"fmt"
	"os"
	"testing"
	"time"

	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"
	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
)

type IntegrationTestFixture struct {
	Environment string
	UserId      string
	DbClient    *dynamodb.Client
	TableName   string
}

var integrationTestFixture IntegrationTestFixture

func TestMain(m *testing.M) {
	testUserId := "auth0|integrationTest"
	environment := "dev"

	cfg := awsConfig.GetConfig(environment)

	integrationTestFixture = IntegrationTestFixture{
		Environment: environment,
		UserId:      testUserId,
		DbClient:    dynamodb.NewFromConfig(cfg),
		TableName:   fmt.Sprintf("MoneyMate_TransactionDB_%v", environment),
	}

	exitVal := m.Run()
	os.Exit(exitVal)
}

func CreateTableMoneyMateDb() {
	out, err := integrationTestFixture.DbClient.CreateTable(context.TODO(), &dynamodb.CreateTableInput{
		TableName: &integrationTestFixture.TableName,
		AttributeDefinitions: []types.AttributeDefinition{
			{
				AttributeName: aws.String("UserIdQuery"),
				AttributeType: types.ScalarAttributeTypeS,
			},
			{
				AttributeName: aws.String("Subquery"),
				AttributeType: types.ScalarAttributeTypeS,
			},
		},
		KeySchema: []types.KeySchemaElement{
			{
				AttributeName: aws.String("UserIdQuery"),
				KeyType:       types.KeyTypeHash,
			},
			{
				AttributeName: aws.String("Subquery"),
				KeyType:       types.KeyTypeRange,
			},
		},
		BillingMode: types.BillingModePayPerRequest,
	})

	if err != nil {
		panic(err)
	}

	fmt.Printf("Finished creating table, %v\n", out)
}

func DeleteMoneyMateDb() {
	out, err := integrationTestFixture.DbClient.DeleteTable(context.TODO(), &dynamodb.DeleteTableInput{
		TableName: &integrationTestFixture.TableName,
	})

	if err != nil {
		panic(err)
	}

	fmt.Printf("Finished deleting table, %v\n", out)
}

func InsertItemIntoMoneyMateDb(item interface{}) {
	dynamodbItem, err := attributevalue.MarshalMap(item)

	if err != nil {
		panic(err)
	}

	_, err = integrationTestFixture.DbClient.PutItem(context.TODO(), &dynamodb.PutItemInput{
		Item:      dynamodbItem,
		TableName: &integrationTestFixture.TableName,
	})

	if err != nil {
		panic(err)
	}
}

func GetAllItemsFromMoneyMateDb[T interface{}]() []T {
	items, err := integrationTestFixture.DbClient.Scan(context.TODO(), &dynamodb.ScanInput{
		TableName: &integrationTestFixture.TableName,
	})

	if err != nil {
		panic(err)
	}

	var unmarshalledItems = make([]T, 0)
	attributevalue.UnmarshalListOfMaps(items.Items, &unmarshalledItems)
	return unmarshalledItems
}

func Test_Integration(t *testing.T) {
	defer DeleteMoneyMateDb()

	CreateTableMoneyMateDb()

	oldCategory := "old/category"
	subcategory := "subcategory"
	newCategory := "new category!"

	transactions := []models.Transaction{
		{
			UserIdQuery:          fmt.Sprintf("%s#Transaction", integrationTestFixture.UserId),
			Subquery:             uuid.NewString(),
			TransactionTimestamp: time.Now().Format("2016-02-01T15:04:05Z"),
			TransactionType:      "expense",
			Amount:               "1235.4",
			Category:             oldCategory,
			SubCategory:          subcategory,
			PayerPayeeId:         uuid.NewString(),
			PayerPayeeName:       "name1",
			Note:                 "note1",
		},
		{
			UserIdQuery:          fmt.Sprintf("%s#Transaction", integrationTestFixture.UserId),
			Subquery:             uuid.NewString(),
			TransactionTimestamp: time.Now().Format("2016-02-02T15:04:05Z"),
			TransactionType:      "expense",
			Amount:               "1235.5",
			Category:             oldCategory,
			SubCategory:          subcategory,
			PayerPayeeId:         uuid.NewString(),
			PayerPayeeName:       "name2",
			Note:                 "note2",
		},
		{
			UserIdQuery:          fmt.Sprintf("%s#Transaction", integrationTestFixture.UserId),
			Subquery:             uuid.NewString(),
			TransactionTimestamp: time.Now().Format("2016-02-03T15:04:05Z"),
			TransactionType:      "expense",
			Amount:               "1235.5",
			Category:             oldCategory,
			SubCategory:          subcategory,
			PayerPayeeId:         uuid.NewString(),
			PayerPayeeName:       "name3",
			Note:                 "note3",
		},
		{
			UserIdQuery:          fmt.Sprintf("%s#Transaction", integrationTestFixture.UserId),
			Subquery:             uuid.NewString(),
			TransactionTimestamp: time.Now().Format("2016-02-04T15:04:05Z"),
			TransactionType:      "income",
			Amount:               "1235.5",
			Category:             "other category",
			SubCategory:          subcategory,
			PayerPayeeId:         uuid.NewString(),
			PayerPayeeName:       "name4",
		},
	}

	for _, transaction := range transactions {
		InsertItemIntoMoneyMateDb(transaction)
	}

	startCategoryModifier(Parameters{
		environment:     integrationTestFixture.Environment,
		userId:          integrationTestFixture.UserId,
		oldCatgoryName:  oldCategory,
		newCategoryName: newCategory,
	})

	expectedTransactions := make([]models.Transaction, len(transactions))
	copy(expectedTransactions, transactions)

	for i := range expectedTransactions {
		if i != (len(expectedTransactions) - 1) {
			expectedTransactions[i].Category = newCategory
		}
	}

	scannedTransactions := GetAllItemsFromMoneyMateDb[models.Transaction]()

	assert.ElementsMatch(t, expectedTransactions, scannedTransactions)
}
