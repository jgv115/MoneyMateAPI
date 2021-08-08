package category_provider

import (
	"categoryInitialiser/models"
	"encoding/json"
)

type CategoryProvider interface {
	GetCategories() ([]models.CategoryDto, error)
}

type JsonCategoryProvider struct {
	CategoryJsonBytes map[string][]byte
}

func (j *JsonCategoryProvider) convertCategoryMapToCategory(categoryType string, inputMap map[string][]string) (categories []models.CategoryDto) {
	for categoryName, subcategories := range inputMap {
		categories = append(categories, models.CategoryDto{
			CategoryName:  categoryName,
			CategoryType:  categoryType,
			Subcategories: subcategories,
		})
	}

	return
}

func (j *JsonCategoryProvider) GetCategories() (categoryModels []models.CategoryDto, err error) {
	for categoryType, stringBytes := range j.CategoryJsonBytes {
		var categoriesMap = make(map[string][]string)
		err := json.Unmarshal(stringBytes, &categoriesMap)
		if err != nil {
			return nil, err
		}

		categoryModels = append(categoryModels, j.convertCategoryMapToCategory(categoryType, categoriesMap)...)
	}

	return
}