using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Dtos;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace TransactionService.IntegrationTests.CategoriesEndpoint;

[Collection("IntegrationTests")]
public class CategoriesEndpointTests : IClassFixture<MoneyMateApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly DynamoDbHelper _dynamoDbHelper;
    private const string PersistedUserId = "auth0|moneymatetest#Categories";
    private const string ConsumerUserId = "auth0|moneymatetest";

    public CategoriesEndpointTests(MoneyMateApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateDefaultClient();
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
                        UserId = ConsumerUserId,
                        CategoryName = "category1",
                        Subcategories = new List<string> {"subcategory1", "subcategory2"}
                    },
                    new()
                    {
                        UserId = ConsumerUserId,
                        CategoryName = "category2",
                        Subcategories = new List<string> {"subcategory3", "subcategory4"}
                    }
                }
            },
            new object[]
            {
                "?transactionType=expense",
                new List<Category>
                {
                    new()
                    {
                        UserId = ConsumerUserId,
                        CategoryName = "category1",
                        Subcategories = new List<string> {"subcategory1", "subcategory2"}
                    }
                }
            },
            new object[]
            {
                "?transactionType=income",
                new List<Category>
                {
                    new()
                    {
                        UserId = ConsumerUserId,
                        CategoryName = "category2",
                        Subcategories = new List<string> {"subcategory3", "subcategory4"}
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
                UserId = PersistedUserId,
                CategoryName = "category1",
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                UserId = PersistedUserId,
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _dynamoDbHelper.WriteIntoTable(initialData);

        var response = await _httpClient.GetAsync($"/api/categories{queryString}");
        var returnedString = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var returnedCategoriesList = JsonSerializer.Deserialize<List<Category>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        for (var i = 0; i < expectedCategories.Count; i++)
        {
            Assert.Equal(expectedCategories[i].CategoryName, returnedCategoriesList![i].CategoryName);
            Assert.Equal(expectedCategories[i].UserId, returnedCategoriesList[i].UserId);
            Assert.Equal(expectedCategories[i].Subcategories, returnedCategoriesList[i].Subcategories);
        }
    }

    [Fact]
    public async Task GivenValidRequest_WhenGetSubcategoriesIsCalled_ThenAllSubcategoriesAreReturned()
    {
        var inputCategories = new List<string> {"category1", "category2", "category3"};
        var expectedSubcategories = new List<string> {"test1", "test2", "test4"};

        await _dynamoDbHelper.WriteIntoTable(inputCategories.Select(category => new Category
        {
            UserId = PersistedUserId,
            CategoryName = category,
            TransactionType = TransactionType.Expense,
            Subcategories = expectedSubcategories
        }));

        var response = await _httpClient.GetAsync("/api/categories/category1");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedSubcategoriesList = JsonSerializer.Deserialize<List<string>>(returnedString);

        Assert.Equal(expectedSubcategories, returnedSubcategoriesList);
    }

    [Fact]
    public async Task GivenAValidCreateCategoryRequest_WhenCreateCategoryIsCalled_ThenCategoryIsPersisted()
    {
        const string inputCategoryName = "category123";
        var inputCategoryType = TransactionType.Expense;
        var expectedSubcategories = new List<string> {"test1", "test2"};

        var inputDto = new CategoryDto
        {
            CategoryName = inputCategoryName,
            TransactionType = inputCategoryType,
            Subcategories = expectedSubcategories
        };

        var httpContent = new StringContent(JsonSerializer.Serialize(inputDto), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/categories", httpContent);

        response.EnsureSuccessStatusCode();

        var returnedCategories = await _dynamoDbHelper.ScanTable<Category>();

        Assert.Collection(returnedCategories, category =>
        {
            Assert.Equal(inputCategoryName, category.CategoryName);
            Assert.Equal(expectedSubcategories, category.Subcategories);
            Assert.Equal(PersistedUserId, category.UserId);
        });
    }

    [Fact]
    public async Task
        GivenAnCreateCategoryRequestForACategoryThatExists_WhenCreateCategoryIsCalled_ThenCategoryIsNotPersisted()
    {
        const string duplicateCategoryName = "category123";
        var duplicateTransactionType = TransactionType.Expense;

        var initialData = new List<Category>
        {
            new()
            {
                UserId = PersistedUserId,
                CategoryName = duplicateCategoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                UserId = PersistedUserId,
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _dynamoDbHelper.WriteIntoTable(initialData);

        var inputDto = new CategoryDto
        {
            CategoryName = duplicateCategoryName,
            TransactionType = duplicateTransactionType,
            Subcategories = new List<string> {"subcategory1", "subcategory2"}
        };

        var httpContent = new StringContent(JsonSerializer.Serialize(inputDto), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/categories", httpContent);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }


    [Fact]
    public async Task
        GivenAPatchCategoryRequestForACategoryThatExists_WhenPatchCategoryIsCalled_ThenCategoryIsUpdated()
    {
        const string categoryName = "category123";

        var initialData = new List<Category>
        {
            new()
            {
                UserId = PersistedUserId,
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                UserId = PersistedUserId,
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _dynamoDbHelper.WriteIntoTable(initialData);


        var inputPatchDoc = "[{ \"op\": \"add\", \"path\": \"/subcategories/-\", \"value\": \"test subcategory\" }]";

        var httpContent = new StringContent(inputPatchDoc,
            Encoding.UTF8, "application/json-patch+json");
        var response = await _httpClient.PatchAsync($"/api/categories/{categoryName}", httpContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var returnedCategory = await _dynamoDbHelper.QueryTable<Category>(PersistedUserId, categoryName);

        Assert.Equal(new Category
        {
            UserId = PersistedUserId,
            CategoryName = categoryName,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"subcategory1", "subcategory2", "test subcategory"}
        }, returnedCategory);
    }
}