package store

import (
	"categoryInitialiser/models"
	"context"
	"fmt"

	"github.com/jackc/pgx/v5"
)

type CockroachDbCategoriesRepository struct {
	Connection *pgx.Conn
	UserId     string
	ProfileId  string
}

func (c *CockroachDbCategoriesRepository) SaveCategories(categories []models.Category) error {

	c.Connection.Exec(context.Background(), "TRUNCATE category CASCADE")

	for _, category := range categories {

		var createdCategoryId string
		err := c.Connection.QueryRow(context.Background(),
			`WITH input (category_name, user_id, transaction_type_name, profile_id) as (VALUES($1::VARCHAR, $2::UUID, $3::VARCHAR, $4::UUID))
			INSERT INTO category (name, user_id, transaction_type_id, profile_id)
			SELECT input.category_name, input.user_id, tt.id, input.profile_id
			FROM input
			LEFT JOIN transactiontype tt ON tt.name = input.transaction_type_name
			RETURNING id`, category.CategoryName, c.UserId, category.TransactionType, c.ProfileId,
		).Scan(&createdCategoryId)

		if err != nil {
			return err
		}

		fmt.Printf("%v", createdCategoryId)

		for _, subcategory := range category.Subcategories {
			c.Connection.Exec(context.Background(),
				`INSERT INTO subcategory (name, category_id) VALUES($1, $2)`, subcategory, createdCategoryId)
		}
	}

	return nil
}
