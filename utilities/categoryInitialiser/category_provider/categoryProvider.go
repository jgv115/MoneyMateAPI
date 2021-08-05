package category_provider

import (
	"categoryInitialiser/models"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"os"
)

type CategoryProvider interface {
	GetCategories() ([]models.CategoryDto, error)
}

type JsonCategoryProvider struct {
	CategoryJsonBytes map[string][]byte
}

func (j *JsonCategoryProvider) readCategoryJson(filePath string) (categories map[string][]string, err error) {
	output, _ := os.Getwd()
	fmt.Println(output)
	categoriesJson, err := os.Open(filePath)
	if err != nil {
		return
	}

	byteValue, err := ioutil.ReadAll(categoriesJson)
	if err != nil {
		return
	}

	err = json.Unmarshal(byteValue, &categories)
	if err != nil {
		return
	}

	return
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