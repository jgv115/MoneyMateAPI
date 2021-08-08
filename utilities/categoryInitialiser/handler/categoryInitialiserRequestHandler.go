package handler

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
			UserIdQuery: fmt.Sprintf("auth0|%v#Categories", userId),
			Subquery:      fmt.Sprintf("%vCategory#%v", categoryDto.CategoryType, categoryDto.CategoryName),
			SubCategories: categoryDto.Subcategories,
		})
	}

	err = categoriesRepository.SaveCategories(categories)
	if err != nil {
		return err
	}
	return nil
}
