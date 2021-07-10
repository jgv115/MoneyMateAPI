using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using TransactionService.Dtos;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.Models;
using Xunit;

namespace TransactionService.IntegrationTests.CategoriesEndpoint
{
    [Collection("IntegrationTests")]
    public class CategoriesEndpointTests : IClassFixture<WebApplicationFactory<Startup>>, IAsyncLifetime
    {
        private readonly HttpClient _httpClient;
        private readonly DynamoDbHelper _dynamoDbHelper;
        private const string UserId = "auth0|moneymatetest#Categories";

        public CategoriesEndpointTests(WebApplicationFactory<Startup> factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.GetAccessToken();
            _dynamoDbHelper = new DynamoDbHelper();
        }

        public async Task InitializeAsync()
        {
            await _dynamoDbHelper.CreateTable();
        }

        public async Task DisposeAsync()
        {
            await _dynamoDbHelper.DeleteTable();
        }

        public static IEnumerable<object[]> CategoriesEndpointTestData =>
            new List<object[]>
            {
                new object[]
                {
                    "",
                    new List<Category>
                    {
                        new()
                        {
                            UserId = UserId,
                            CategoryName = "category1",
                            SubCategories = new List<string> {"subcategory1", "subcategory2"}
                        },
                        new()
                        {
                            UserId = UserId,
                            CategoryName = "category2",
                            SubCategories = new List<string> {"subcategory3", "subcategory4"}
                        }
                    }
                },
                new object[]
                {
                    "?categoryType=expense",
                    new List<Category>
                    {
                        new()
                        {
                            UserId = UserId,
                            CategoryName = "category1",
                            SubCategories = new List<string> {"subcategory1", "subcategory2"}
                        }
                    }
                },
                new object[]
                {
                    "?categoryType=income",
                    new List<Category>
                    {
                        new()
                        {
                            UserId = UserId,
                            CategoryName = "category2",
                            SubCategories = new List<string> {"subcategory3", "subcategory4"}
                        }
                    }
                }
            };


        [Theory]
        [MemberData(nameof(CategoriesEndpointTestData))]
        public async Task GivenValidRequest_WhenGetCategoriesIsCalledWithCategoryType_ThenAllCategoriesAreReturned(
            string queryString, List<Category> expectedCategories)
        {
            var initialData = new List<Category>
            {
                new()
                {
                    UserId = UserId,
                    CategoryName = "expenseCategory#category1",
                    SubCategories = new List<string> {"subcategory1", "subcategory2"}
                },
                new()
                {
                    UserId = UserId,
                    CategoryName = "incomeCategory#category2",
                    SubCategories = new List<string> {"subcategory3", "subcategory4"}
                }
            };

            await _dynamoDbHelper.WriteIntoTable(initialData);

            var response = await _httpClient.GetAsync($"/api/categories{queryString}");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedCategoriesList = JsonSerializer.Deserialize<List<Category>>(returnedString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            for (var i = 0; i < expectedCategories.Count; i++)
            {
                Assert.Equal(expectedCategories[i].CategoryName, returnedCategoriesList[i].CategoryName);
                Assert.Equal(expectedCategories[i].UserId, returnedCategoriesList[i].UserId);
                Assert.Equal(expectedCategories[i].SubCategories, returnedCategoriesList[i].SubCategories);
            }
        }

        [Fact]
        public async Task GivenValidRequest_WhenGetSubCategoriesIsCalled_ThenAllSubCategoriesAreReturned()
        {
            var expectedCategories = new List<string> {"category1", "category2", "category3"};
            var expectedSubCategories = new List<string> {"test1", "test2", "test4"};

            await _dynamoDbHelper.WriteIntoTable(expectedCategories.Select(category => new Category
            {
                UserId = UserId,
                CategoryName = category,
                SubCategories = expectedSubCategories
            }));

            var response = await _httpClient.GetAsync("/api/categories/category1");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedCategoriesList = JsonSerializer.Deserialize<List<string>>(returnedString);

            Assert.Equal(expectedSubCategories, returnedCategoriesList);
        }

        [Fact]
        public async Task GivenAValidExpenseCategoryRequest_WhenCreateCategoryIsCalled_ThenCategoryIsPersisted()
        {
            const string inputCategoryName = "category123";
            const string inputCategoryType = "expense";
            var expectedCategoryName = $"{inputCategoryType}Category#{inputCategoryName}";
            var expectedSubcategories = new List<string> {"test1", "test2"};

            var inputDto = new CreateCategoryDto
            {
                CategoryName = inputCategoryName,
                CategoryType = inputCategoryType,
                SubCategories = expectedSubcategories
            };

            var httpContent = new StringContent(JsonSerializer.Serialize(inputDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/categories", httpContent);

            response.EnsureSuccessStatusCode();

            var returnedCategories = await _dynamoDbHelper.ScanTable<Category>();

            Assert.Single(returnedCategories);
            Assert.Equal(expectedCategoryName, returnedCategories[0].CategoryName);
            Assert.Equal(expectedSubcategories, returnedCategories[0].SubCategories);
            Assert.Equal(UserId, returnedCategories[0].UserId);
        }
    }
}