// +build integrationTest

package integrationTests

import (
	"categoryInitialiser/models"
	"encoding/json"
	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/lambda"
	"github.com/stretchr/testify/assert"
	"os"
	"testing"
)

var awsFixture struct {
	session        *session.Session
	lambdaClient   *lambda.Lambda
	dynamoDbClient *dynamodb.DynamoDB
}

func TestMain(m *testing.M) {

	awsFixture.session = session.Must(session.NewSession(&aws.Config{
		Endpoint: aws.String("http://localstack:4566"),
		Region:   aws.String("ap-southeast-2"),
	}))

	awsFixture.lambdaClient = lambda.New(awsFixture.session)
	awsFixture.dynamoDbClient = dynamodb.New(awsFixture.session)

	code := m.Run()
	os.Exit(code)
}

func TestCategoryInitialiser(t *testing.T) {
	t.Run("given input payload, when Lambda invoked, then correct categories persisted in DynamoDB", func(t *testing.T) {
		const expectedUserId = "TestUser123"
		input := models.IntialiseCategoriesRequest{
			Id:                  expectedUserId,
			Tenant:              "",
			Username:            "",
			Email:               "",
			EmailVerified:       false,
			PhoneNumber:         "",
			PhoneNumberVerified: false,
			UserMetadata: struct {
				Hobby string `json:"hobby"`
			}{},
			AppMetadata: struct {
				Plan string `json:"plan"`
			}{},
		}

		inputString, _ := json.Marshal(input)

		_, err := awsFixture.lambdaClient.Invoke(&lambda.InvokeInput{
			FunctionName: aws.String("category_initialiser_lambda"),
			Payload:      inputString,
		})

		if err != nil {
			t.Errorf("Got error %+v when not expecting one", err)
		}

		scanOutput, _ := awsFixture.dynamoDbClient.Scan(&dynamodb.ScanInput{
			TableName: aws.String("MoneyMate_TransactionDB_dev"),
		})

		assert.Len(t, scanOutput.Items, 8)
	})
}
