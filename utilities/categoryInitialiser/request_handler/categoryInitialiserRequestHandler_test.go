//go:build !integrationTest
// +build !integrationTest

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
		const userId = "testUserId123"

		err := HandleRequest(userId, mockCategoryProvider, mockCategoriesRepository)
		if err != nil {
			t.Errorf("HandleRequest returned error %+v when not expecting error", err)
		}

		mockCategoryProvider.AssertNumberOfCalls(t, "GetCategories", 1)
	})

	t.Run("given valid inputs, when HandleRequest called, then SaveCategories called with correct inputs", func(t *testing.T) {
		var mockCategoryProvider = new(MockCategoryProvider)
		var mockCategoriesRepository = new(MockCategoryRepository)

		const userId = "testUserId123"
		const expectedCategoryName1 = "Category1"
		var expectedSubcategories1 = []string{"Subcategory1"}
		const expectedCategoryName2 = "Category2"
		var expectedSubcategories2 = []string{"Subcategory2"}
		var categoryDtos = []models.CategoryDto{
			{
				CategoryName:  expectedCategoryName1,
				CategoryType:  "expense",
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

		err := HandleRequest(userId, mockCategoryProvider, mockCategoriesRepository)
		if err != nil {
			t.Errorf("HandleRequest returned error %+v when not expecting error", err)
		}

		var expectedCategories = []models.Category{
			{
				UserIdQuery:     "auth0|" + userId + "#Categories",
				Subquery:        expectedCategoryName1,
				Subcategories:   expectedSubcategories1,
				TransactionType: 0,
			},
			{
				UserIdQuery:     "auth0|" + userId + "#Categories",
				Subquery:        expectedCategoryName2,
				Subcategories:   expectedSubcategories2,
				TransactionType: 1,
			},
		}

		mockCategoriesRepository.AssertCalled(t, "SaveCategories", expectedCategories)
	})
}
