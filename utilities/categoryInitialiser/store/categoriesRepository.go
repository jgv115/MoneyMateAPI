package store

import "categoryInitialiser/models"

type CategoriesRepository interface {
	SaveCategories([]models.Category) error
}
