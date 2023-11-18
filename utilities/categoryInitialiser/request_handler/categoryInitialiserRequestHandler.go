package request_handler

import (
	"categoryInitialiser/category_provider"
	"categoryInitialiser/models"
	"categoryInitialiser/store"
	"strings"
)

func HandleRequest(categoryProvider category_provider.CategoryProvider, categoriesRepository store.CategoriesRepository) error {
	categoryDtos, err := categoryProvider.GetCategories()
	if err != nil {
		return err
	}

	categories := make([]models.Category, 0)

	for _, categoryDto := range categoryDtos {

		categories = append(categories, models.Category{
			CategoryName:    categoryDto.CategoryName,
			Subcategories:   categoryDto.Subcategories,
			TransactionType: strings.ToLower(categoryDto.CategoryType),
		})
	}

	err = categoriesRepository.SaveCategories(categories)
	if err != nil {
		return err
	}
	return nil
}
