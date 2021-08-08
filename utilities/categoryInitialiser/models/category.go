package models

type Category struct {
	UserIdQuery string
	Subquery string
	SubCategories []string `dynamodbav:",stringset"`
}
