package request_handler

import (
	"categoryInitialiser/category_provider"
	"categoryInitialiser/models"
	"categoryInitialiser/store"
	"fmt"
)

func HandleRequest(userId string, categoryProvider category_provider.CategoryProvider, categoriesRepository store.CategoriesRepository) error {
	categoryDtos, err := categoryProvider.GetCategories()
	if err != nil {
		return err
	}

	categories := make([]models.Category, 0)

	for _, categoryDto := range categoryDtos {

		categories = append(categories, models.Category{
			UserIdQuery:     fmt.Sprintf("auth0|%v#Categories", userId),
			Subquery:        categoryDto.CategoryName,
			SubCategories:   categoryDto.Subcategories,
			TransactionType: mapTransactionTypeStringToInt(categoryDto.CategoryType),
		})
	}

	err = categoriesRepository.SaveCategories(categories)
	if err != nil {
		return err
	}
	return nil
}

func mapTransactionTypeStringToInt(transactionType string) int {
	if transactionType == "expense" {
		return 0
	} else {
		return 1
	}
}
