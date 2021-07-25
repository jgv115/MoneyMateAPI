package services

import (
	"fmt"
	"net/http"
	"net/url"
)

type CategoryInitialiserService interface {
	Initialise(userId string) error
}

type CategoriesApiCategoryInitialiserService struct {
	HttpClient *http.Client
	BaseUrl    *url.URL
}

func (c *CategoriesApiCategoryInitialiserService) Initialise(userId string) error {

	var tempUrl = &url.URL{
		Path: "/api/test",
	}
	var finalUrl = c.BaseUrl.ResolveReference(tempUrl).String()
	fmt.Println(finalUrl)

	//response, err:= c.HttpClient.Post()


	return nil
}
