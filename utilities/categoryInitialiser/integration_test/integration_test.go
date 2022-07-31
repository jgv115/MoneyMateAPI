//go:build integrationTest
// +build integrationTest

package integrationTests

import (
	"categoryInitialiser/aws_utils"
	"categoryInitialiser/models"
	"encoding/json"
	"fmt"
	"os"
	"testing"

	"github.com/aws/aws-sdk-go/aws"
	"github.com/aws/aws-sdk-go/aws/session"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/lambda"
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

		inputString, err := json.Marshal(input)

		if err != nil {
			t.Fatalf("Got error %+v when marshaling json", err)
		}

		output, err := awsFixture.lambdaClient.Invoke(&lambda.InvokeInput{
			FunctionName:   aws.String("category_initialiser_lambda"),
			Payload:        inputString,
			LogType:        aws.String("Tail"),
			InvocationType: aws.String("RequestResponse"),
		})

		fmt.Printf("%+v", string(output.Payload))

		if err != nil {
			t.Fatalf("Got error %+v when not expecting one", err)
		}
		// time.Sleep(time.Hour)
		scanOutput, _ := awsFixture.dynamoDbClient.Scan(&dynamodb.ScanInput{
			TableName: aws.String("MoneyMate_TransactionDB_dev"),
		})

		assert.Len(t, scanOutput.Items, 9)
	})
}
