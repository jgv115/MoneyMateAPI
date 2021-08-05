package store

import (
	"categoryInitialiser/models"
	"github.com/aws/aws-sdk-go/service/dynamodb"
	"github.com/aws/aws-sdk-go/service/dynamodb/dynamodbattribute"
	"github.com/aws/aws-sdk-go/service/dynamodb/dynamodbiface"
)

type DynamoDbCategoriesRepository struct {
	DynamoDbClient dynamodbiface.DynamoDBAPI
	TableName      string
}

func (d *DynamoDbCategoriesRepository) SaveCategories(categories []models.Category) error {

	dynamoDbWriteRequests := make([]*dynamodb.WriteRequest, 0)
	for _, category := range categories {
		item, err := dynamodbattribute.MarshalMap(category)
		if err != nil {
			return err
		}
		dynamoDbWriteRequests = append(dynamoDbWriteRequests, &dynamodb.WriteRequest{
			PutRequest: &dynamodb.PutRequest{
				Item: item,
			},
		})
	}

	_, err := d.DynamoDbClient.BatchWriteItem(&dynamodb.BatchWriteItemInput{RequestItems: map[string][]*dynamodb.WriteRequest{
		d.TableName: dynamoDbWriteRequests,
	}})
	if err != nil {
		return err
	}

	return nil
}
