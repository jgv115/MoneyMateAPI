package repository

import (
	"categoryModifier/models"
	"context"
	"fmt"

	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"
)

type MoneyMateDbRepository interface {
	GetTransactionsWithCategory(category string) ([]models.Transaction, error)
	UpdateTransactionWithNewCategory(transactionId string, newCategory string) error
}

type DynamoDbMoneyMateDbRepository struct {
	UserId    string
	Client    *dynamodb.Client
	TableName string
}

func (d DynamoDbMoneyMateDbRepository) getTransactionPartitionKey() string {
	return fmt.Sprintf("%s#Transaction", d.UserId)
}

func (d DynamoDbMoneyMateDbRepository) GetTransactionsWithCategory(category string) ([]models.Transaction, error) {
	paginator := dynamodb.NewQueryPaginator(d.Client, &dynamodb.QueryInput{
		TableName:              &d.TableName,
		KeyConditionExpression: aws.String("UserIdQuery = :userIdQuery"),
		FilterExpression:       aws.String("Category = :category"),
		ExpressionAttributeValues: map[string]types.AttributeValue{
			":userIdQuery": &types.AttributeValueMemberS{Value: d.getTransactionPartitionKey()},
			":category":    &types.AttributeValueMemberS{Value: category},
		},
	})

	var transactions []models.Transaction

	for paginator.HasMorePages() {
		queryOutput, err := paginator.NextPage(context.TODO())
		if err != nil {
			return nil, err
		}

		err = attributevalue.UnmarshalListOfMaps(queryOutput.Items, &transactions)
		if err != nil {
			return nil, err
		}
	}

	return transactions, nil
}

func (d DynamoDbMoneyMateDbRepository) UpdateTransactionWithNewCategory(transactionId string, newCategory string) error {
	_, err := d.Client.UpdateItem(context.TODO(), &dynamodb.UpdateItemInput{
		TableName: &d.TableName,
		Key: map[string]types.AttributeValue{
			"UserIdQuery": &types.AttributeValueMemberS{
				Value: d.getTransactionPartitionKey(),
			},
			"Subquery": &types.AttributeValueMemberS{
				Value: transactionId,
			},
		},
		UpdateExpression: aws.String("SET Category = :newCategory"),
		ExpressionAttributeValues: map[string]types.AttributeValue{
			":newCategory": &types.AttributeValueMemberS{
				Value: newCategory,
			},
		},
	})

	return err
}
