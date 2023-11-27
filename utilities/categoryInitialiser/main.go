package main

import (
	"categoryInitialiser/category_provider"
	"categoryInitialiser/models"
	"categoryInitialiser/request_handler"
	"categoryInitialiser/store"
	"context"
	_ "embed"
	"errors"
	"fmt"
	"log"
	"os"

	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/ssm"
	"github.com/jackc/pgx/v5"
)

func Handle(request models.IntialiseCategoriesRequest) (err error) {
	categoryProvider, categoriesRepository, err := setupDependencies(request)
	if err != nil {
		return err
	}

	err = request_handler.HandleRequest(categoryProvider, categoriesRepository)
	if err != nil {
		return err
	}

	return
}

//go:embed category_provider/data/expenseCategories.json
var expenseCategories []byte

//go:embed category_provider/data/incomeCategories.json
var incomeCategories []byte

func setupDependencies(request models.IntialiseCategoriesRequest) (categoryProvider category_provider.CategoryProvider, categoriesRepository store.CategoriesRepository, err error) {
	environment, ok := os.LookupEnv("ENVIRONMENT")
	if !ok {
		err = errors.New("no ENVIRONMENT environment variable found")
		return
	}

	var cockroachDbConnectionString string
	if environment == "dev" {
		var envVar = "CATEGORY_INITIALISER_COCKROACHDB_CONNECTION_STRING"
		cockroachDbConnectionString, ok = os.LookupEnv(envVar)
		if !ok {
			err = errors.New("no CockroachDb connection string environment variable found")
			return
		}
	} else {
		cfg, err := config.LoadDefaultConfig(context.TODO())
		if err != nil {
			panic("configuration error, " + err.Error())
		}
		client := ssm.NewFromConfig(cfg)

		parameterResults, err := client.GetParameter(context.Background(), &ssm.GetParameterInput{
			Name:           aws.String(fmt.Sprintf("/%s/categoryInitialiser/cockroachDbConnectionString", environment)),
			WithDecryption: aws.Bool(true),
		})

		if err != nil {
			log.Fatal(err)
		}
		cockroachDbConnectionString = *parameterResults.Parameter.Value
	}

	cockroachDbConnection, err := pgx.Connect(context.Background(), cockroachDbConnectionString)
	if err != nil {
		return nil, nil, err
	}

	categoriesRepository = &store.CockroachDbCategoriesRepository{
		Connection: cockroachDbConnection,
		UserId:     request.UserId,
		ProfileId:  request.ProfileId,
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
	lambda.Start(Handle)
}
