package main

import (
	"categoryInitialiser/models"
	"categoryInitialiser/services"
	"fmt"
	"github.com/aws/aws-lambda-go/lambda"
	"net/http"
	"net/url"
)

func handle(request models.IntialiseCategoriesRequest) {
	var initialiserService = &services.CategoriesApiCategoryInitialiserService {
		HttpClient: &http.Client{},
		BaseUrl: &url.URL{
			Scheme: "https",
			Host:   "api.test.moneymate.benong.id.au",
		},
	}

	_ = initialiserService.Initialise("userId")
	fmt.Println("hello")
	fmt.Printf("%+v\n", request)
}

func main() {
	lambda.Start(handle)
}