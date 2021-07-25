package services

import (
	"net/url"
	"testing"
)

func TestCategoriesApiCategoryInitialiserService_Initialise(t *testing.T) {
	t.Run("given test", func(t *testing.T) {
		var service = &CategoriesApiCategoryInitialiserService{
			HttpClient: nil,
			BaseUrl:    &url.URL{
				Scheme: "https",
				Host: "api.test.moneymate.benong.id.au",
			},
		}

		_ = service.Initialise("test-user")
	})
}
