//go:build !integrationTest

package request_handler

import (
	"categoryInitialiser/models"
	"testing"

	"github.com/stretchr/testify/mock"
)

type MockCategoryProvider struct {
	mock.Mock
}

func (p *MockCategoryProvider) GetCategories() ([]models.CategoryDto, error) {
	args := p.Called()
	return args.Get(0).([]models.CategoryDto), args.Error(1)
}

type MockCategoryRepository struct {
	mock.Mock
}

func (r *MockCategoryRepository) SaveCategories(categories []models.Category) error {
	args := r.Called(categories)
	return args.Error(0)
}

func TestHandleRequest(t *testing.T) {
	t.Run("given valid inputs, when HandleRequest called, then GetCategories called once", func(t *testing.T) {
		var mockCategoryProvider = new(MockCategoryProvider)
		var mockCategoriesRepository = new(MockCategoryRepository)

		mockCategoryProvider.On("GetCategories").Return([]models.CategoryDto{}, nil)
		mockCategoriesRepository.On("SaveCategories", mock.Anything).Return(nil)

		err := HandleRequest(mockCategoryProvider, mockCategoriesRepository)
		if err != nil {
			t.Errorf("HandleRequest returned error %+v when not expecting error", err)
		}

		mockCategoryProvider.AssertNumberOfCalls(t, "GetCategories", 1)
	})

	t.Run("given valid inputs, when HandleRequest called, then SaveCategories called with correct inputs", func(t *testing.T) {
		var mockCategoryProvider = new(MockCategoryProvider)
		var mockCategoriesRepository = new(MockCategoryRepository)

		const expectedCategoryName1 = "Category1"
		var expectedSubcategories1 = []string{"Subcategory1"}
		const expectedCategoryName2 = "Category2"
		var expectedSubcategories2 = []string{"Subcategory2"}
		var categoryDtos = []models.CategoryDto{
			{
				CategoryName:  expectedCategoryName1,
				CategoryType:  "Expense",
				Subcategories: expectedSubcategories1,
			},
			{
				CategoryName:  expectedCategoryName2,
				CategoryType:  "income",
				Subcategories: expectedSubcategories2,
			},
		}

		mockCategoryProvider.On("GetCategories").Return(categoryDtos, nil)
		mockCategoriesRepository.On("SaveCategories", mock.Anything).Return(nil)

		err := HandleRequest( mockCategoryProvider, mockCategoriesRepository)
		if err != nil {
			t.Errorf("HandleRequest returned error %+v when not expecting error", err)
		}

		var expectedCategories = []models.Category{
			{
				CategoryName:    expectedCategoryName1,
				Subcategories:   expectedSubcategories1,
				TransactionType: "expense",
			},
			{
				CategoryName:    expectedCategoryName2,
				Subcategories:   expectedSubcategories2,
				TransactionType: "income",
			},
		}

		mockCategoriesRepository.AssertCalled(t, "SaveCategories", expectedCategories)
	})
}
