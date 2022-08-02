package models

type Category struct {
	UserIdQuery     string
	Subquery        string
	Subcategories   []string `dynamodbav:",stringset"`
	TransactionType int
}
