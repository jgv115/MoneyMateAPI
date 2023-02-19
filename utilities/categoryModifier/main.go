package main

import (
	"fmt"
	"sync"

	"categoryModifier/awsConfig"
	"categoryModifier/repository"

	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
)

type Parameters struct {
	environment     string
	userId          string
	oldCatgoryName  string
	newCategoryName string
}

func initialiseDependencies(parameters Parameters) repository.MoneyMateDbRepository {
	environment := parameters.environment

	cfg := awsConfig.GetConfig(environment)

	moneymateDb := &repository.DynamoDbMoneyMateDbRepository{
		UserId:    parameters.userId,
		Client:    dynamodb.NewFromConfig(cfg),
		TableName: fmt.Sprintf("MoneyMate_TransactionDB_%v", environment),
	}

	return moneymateDb
}

func startCategoryModifier(params Parameters) {

	moneymateDb := initialiseDependencies(params)

	transactions, _ := moneymateDb.GetTransactionsWithCategory(params.oldCatgoryName)

	var transactionIds []string
	for _, transaction := range transactions {
		fmt.Println(transaction)

		if transaction.Category == params.oldCatgoryName {
			transactionIds = append(transactionIds, transaction.Subquery)
		}
	}

	fmt.Println(transactionIds)

	var wg sync.WaitGroup
	for _, transactionId := range transactionIds {
		wg.Add(1)

		go func(transactionId string) {
			defer wg.Done()
			fmt.Println("Modifying category for transactionId", transactionId)
			err := moneymateDb.UpdateTransactionWithNewCategory(transactionId, params.newCategoryName)
			if err != nil {
				fmt.Printf("error occurred updating transactionId: %s, error: %v\n", transactionId, err)
			}
		}(transactionId)
	}

	wg.Wait()
}

func main() {
	environment := "prod"
	userId := "auth0|jgv115"
	interestedCategory := "Entertainment/Eating Out"
	newCategoryName := "Entertainment"

	startCategoryModifier(Parameters{
		environment:     environment,
		userId:          userId,
		oldCatgoryName:  interestedCategory,
		newCategoryName: newCategoryName,
	})
}
