// +build !integrationTest

package store

import (
	"categoryInitialiser/models"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/dynamodb/dynamodbiface"
	"github.com/google/uuid"
	"github.com/stretchr/testify/mock"
	"testing"
)

type MockDynamoDbClient struct {
	mock.Mock
	dynamodbiface.DynamoDBAPI
}

func (m *MockDynamoDbClient) BatchWriteItem(input *dynamodb.BatchWriteItemInput) (*dynamodb.BatchWriteItemOutput, error) {
	args := m.Called(input)
	return args.Get(0).(*dynamodb.BatchWriteItemOutput), args.Error(1)
}

func createCategories(count int) (categories []models.Category) {
	for i := 0; i < count; i++ {
		categories = append(categories, models.Category{
			UserIdQuery: uuid.NewString(),
			Subquery:    "test_subquery",
			SubCategories: []string{
				"test1", "test2",
			},
		})
	}
	return
}

func TestDynamoDbCategoriesRepository_SaveCategories(t *testing.T) {
	t.Run("given input categories when SaveCategories invoked, then BatchWriteItem is called with correct table name", func(t *testing.T) {
		var mockDynamoDbClient = new(MockDynamoDbClient)
		const expectedTableName = "TestTableName"

		mockDynamoDbClient.On("BatchWriteItem", mock.Anything).Return(&dynamodb.BatchWriteItemOutput{}, nil)
		var repository = &DynamoDbCategoriesRepository{
			DynamoDbClient: mockDynamoDbClient,
			TableName:      expectedTableName,
		}

		const numberOfItems = 10
		var expectedCategories = createCategories(numberOfItems)
		_ = repository.SaveCategories(expectedCategories)

		mockDynamoDbClient.AssertNumberOfCalls(t, "BatchWriteItem", 1)

		for _, call := range mockDynamoDbClient.Calls {
			var batchWriteItemInput = call.Arguments.Get(0).(*dynamodb.BatchWriteItemInput)

			if _, ok := batchWriteItemInput.RequestItems[expectedTableName]; !ok {
				t.Errorf("Expected table name: %v to be in WriteRequest but was not", expectedTableName)
			}
		}
	})

	t.Run("given input categories when SaveCategories invoked, then BatchWriteItem is called with RequestItems with SubCategories stringset", func(t *testing.T) {
		var mockDynamoDbClient = new(MockDynamoDbClient)
		const expectedTableName = "TestTableName"

		mockDynamoDbClient.On("BatchWriteItem", mock.Anything).Return(&dynamodb.BatchWriteItemOutput{}, nil)
		var repository = &DynamoDbCategoriesRepository{
			DynamoDbClient: mockDynamoDbClient,
			TableName:      expectedTableName,
		}

		const numberOfItems = 1
		var expectedCategories = createCategories(numberOfItems)
		_ = repository.SaveCategories(expectedCategories)

		mockDynamoDbClient.AssertNumberOfCalls(t, "BatchWriteItem", 1)
		var actualBatchWriteItemInput = mockDynamoDbClient.Calls[0].Arguments.Get(0).(*dynamodb.BatchWriteItemInput)
		subCategoriesDynamoDbItem := actualBatchWriteItemInput.RequestItems[expectedTableName][0].PutRequest.Item["SubCategories"]

		if subCategoriesDynamoDbItem.SS == nil {
			t.Errorf("expected subCategories to be a stringset but was not, subCategories item looks like this: %+v", subCategoriesDynamoDbItem)
		}

	})
}
