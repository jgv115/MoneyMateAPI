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
	"os"
	"strings"

	"github.com/aws/aws-lambda-go/lambda"
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

	var envVar = fmt.Sprintf("CATEGORY_INITIALISER_COCKROACHDB_CONNECTION_STRING_%s", strings.ToUpper(environment))
	cockroachDbConnectionString, ok := os.LookupEnv(envVar)
	if !ok {
		err = errors.New("no CockroachDb connection string environment variable found")
		return
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
