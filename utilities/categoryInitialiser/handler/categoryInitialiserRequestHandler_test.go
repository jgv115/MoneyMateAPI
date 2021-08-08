// +build !integrationTest

package handler

import (
	"categoryInitialiser/models"
	"github.com/stretchr/testify/mock"
	"testing"
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
		const expectedCategoryName = "Category1"
		const expectedCategoryType = "TestType"
		var expectedSubcategories = []string{"Subcategory1"}
		var categoryDtos = []models.CategoryDto{
			{
				CategoryName:  expectedCategoryName,
				CategoryType:  expectedCategoryType,
				Subcategories: expectedSubcategories,
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
				UserIdQuery:   "auth0|" + userId + "#Categories",
				Subquery:      expectedCategoryType + "Category#" + expectedCategoryName,
				SubCategories: expectedSubcategories,
			},
		}

		mockCategoriesRepository.AssertCalled(t, "SaveCategories", expectedCategories)
	})
}
