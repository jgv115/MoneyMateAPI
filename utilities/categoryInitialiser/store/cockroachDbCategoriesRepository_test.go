//go:build integrationTest

package store

import (
	"categoryInitialiser/models"
	"categoryInitialiser/test_utils"
	"context"
	"sort"
	"testing"

	"github.com/jackc/pgx/v5"
	"github.com/stretchr/testify/assert"
)

func Test(t *testing.T) {
	t.Run("given user and profile when saveCategories called then correct items saved into db", func(t *testing.T) {
		dsn := "postgresql://root@localhost:26257/moneymate_db_local?sslmode=disable"
		conn, _ := pgx.Connect(context.Background(), dsn)
		cockroachDbHelpers := &test_utils.CockroachDbHelpers{
			Connection: conn,
		}

		cockroachDbHelpers.ClearData()

		userId, _ := cockroachDbHelpers.CreateUser("golang_test")

		profileId, _ := cockroachDbHelpers.CreateProfile("Default Profile")

		var repo = CockroachDbCategoriesRepository{
			Connection: conn,
			UserId:     userId,
			ProfileId:  profileId,
		}

		inputCategory := models.Category{
			CategoryName: "test",
			Subcategories: []string{
				"sub1", "sub2", "sub3",
			},
			TransactionType: "expense",
		}

		err := repo.SaveCategories([]models.Category{
			inputCategory,
		})

		assert.Nil(t, err)

		var categoryId string
		var name string
		var transaction_type_id string
		var profile_id string

		conn.QueryRow(context.Background(), `SELECT * FROM category`).Scan(&categoryId, &name, &userId, &transaction_type_id, &profile_id)
		assert.Equal(t, inputCategory.CategoryName, name)

		var transaction_type string
		conn.QueryRow(context.Background(), `SELECT name FROM transactiontype WHERE id = $1`, transaction_type_id).Scan(&transaction_type)
		assert.Equal(t, inputCategory.TransactionType, transaction_type)

		subcategoryRows, err := conn.Query(context.Background(), `SELECT name FROM subcategory WHERE category_id = $1`, categoryId)
		assert.Nil(t, err)

		defer subcategoryRows.Close()

		subcategories, err := pgx.CollectRows(subcategoryRows, func(row pgx.CollectableRow) (string, error) {
			var name string
			row.Scan(&name)

			return name, nil
		})
		assert.Nil(t, err)

		sort.Slice(subcategories, func(i, j int) bool {
			return subcategories[i] < subcategories[j]
		})
		assert.Equal(t, inputCategory.Subcategories, subcategories)

	})
}
