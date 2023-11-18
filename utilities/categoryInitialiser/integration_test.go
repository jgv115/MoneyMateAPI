

package main

import (
	"categoryInitialiser/aws_utils"
	"categoryInitialiser/models"
	"categoryInitialiser/test_utils"
	"context"
	"os"
	"testing"

	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/lambda"
	"github.com/jackc/pgx/v5"

	"github.com/stretchr/testify/assert"
)

var awsFixture struct {
	session        *session.Session
	lambdaClient   *lambda.Lambda
	dynamoDbClient *dynamodb.DynamoDB
}

func TestMain(m *testing.M) {
	awsFixture.session = aws_utils.CreateAWSSession("dev")

	awsFixture.lambdaClient = lambda.New(awsFixture.session)
	awsFixture.dynamoDbClient = dynamodb.New(awsFixture.session)

	code := m.Run()
	os.Exit(code)
}

func TestCategoryInitialiser(t *testing.T) {
	t.Run("given input payload, when Lambda invoked, then correct categories persisted in DynamoDB", func(t *testing.T) {
		connectionString := "postgresql://root@localhost:26257/moneymate_db_local?sslmode=disable"
		os.Setenv("ENVIRONMENT", "dev")
		os.Setenv("CATEGORY_INITIALISER_COCKROACHDB_CONNECTION_STRING_DEV", connectionString)

		conn, _ := pgx.Connect(context.Background(), connectionString)
		cockroachDbHelpers := &test_utils.CockroachDbHelpers{
			Connection: conn,
		}

		cockroachDbHelpers.ClearData()

		userId, _ := cockroachDbHelpers.CreateUser("golang_test")

		profileId, _ := cockroachDbHelpers.CreateProfile("Default Profile")

		err := Handle(models.IntialiseCategoriesRequest{
			UserId:    userId,
			ProfileId: profileId,
		})

		assert.Nil(t, err)

		var numberOfCategories int
		conn.QueryRow(context.Background(), `SELECT COUNT(1) from category`).Scan(&numberOfCategories)

		var numberOfSubcategories int
		conn.QueryRow(context.Background(), "SELECT COUNT(1) from subcategory").Scan(&numberOfSubcategories)

		assert.Equal(t, 9, numberOfCategories)
	})
}
