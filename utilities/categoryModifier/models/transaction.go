package models

type Transaction struct {
	UserIdQuery          string
	Subquery             string
	TransactionTimestamp string
	TransactionType      string
	Amount               string
	Category             string
	SubCategory          string
	PayerPayeeId         string
	PayerPayeeName       string
	Note                 string
}
