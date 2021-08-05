// +build !integrationTest

package category_provider

import (
	"categoryInitialiser/models"
	_ "embed"
	"github.com/stretchr/testify/assert"
	"sort"
	"testing"
)

//go:embed data/testCategories.json
var testCategories []byte

func TestJsonCategoryProvider_ParseCategories(t *testing.T) {
	const expectedCategoryType = "TestCategoryType"
	var parser = &JsonCategoryProvider{
		CategoryJsonBytes: map[string][]byte{
			expectedCategoryType: testCategories,
		},
	}

	categories, err := parser.GetCategories()
	sort.Slice(categories, func(i, j int) bool {
		return categories[i].CategoryName > categories[j].CategoryName
	})

	if err != nil {
		return
	}

	var expectedCategories = []models.CategoryDto{
		{
			CategoryName: "Category1",
			CategoryType: expectedCategoryType,
			Subcategories: []string{
				"Subcategory1", "Subcategory2",
			},
		},
		{
			CategoryName: "Category2",
			CategoryType: expectedCategoryType,
			Subcategories: []string{
				"Subcategory1", "Subcategory2",
			},
		},
	}
	sort.Slice(expectedCategories, func(i, j int) bool {
		return expectedCategories[i].CategoryName > expectedCategories[j].CategoryName
	})

	assert.Equal(t, expectedCategories, categories)
}
