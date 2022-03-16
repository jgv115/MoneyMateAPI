using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
    private const string UserId = "auth0|moneymatetest#Categories";

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
                        UserId = UserId,
                        CategoryName = "category1",
                        Subcategories = new List<string> {"subcategory1", "subcategory2"}
                    },
                    new()
                    {
                        UserId = UserId,
                        CategoryName = "category2",
                        Subcategories = new List<string> {"subcategory3", "subcategory4"}
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
                        Subcategories = new List<string> {"subcategory1", "subcategory2"}
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
                UserId = UserId,
                CategoryName = "expenseCategory#category1",
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                UserId = UserId,
                CategoryName = "incomeCategory#category2",
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
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
            Assert.Equal(expectedCategories[i].Subcategories, returnedCategoriesList[i].Subcategories);
        }
    }

    [Fact]
    public async Task GivenValidRequest_WhenGetSubcategoriesIsCalled_ThenAllSubcategoriesAreReturned()
    {
        var expectedCategories = new List<string> {"category1", "category2", "category3"};
        var expectedSubcategories = new List<string> {"test1", "test2", "test4"};

        await _dynamoDbHelper.WriteIntoTable(expectedCategories.Select(category => new Category
        {
            UserId = UserId,
            CategoryName = category,
            Subcategories = expectedSubcategories
        }));

        var response = await _httpClient.GetAsync("/api/categories/category1");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedCategoriesList = JsonSerializer.Deserialize<List<string>>(returnedString);

        Assert.Equal(expectedSubcategories, returnedCategoriesList);
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
            Subcategories = expectedSubcategories
        };

        var httpContent = new StringContent(JsonSerializer.Serialize(inputDto), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/categories", httpContent);

        response.EnsureSuccessStatusCode();

        var returnedCategories = await _dynamoDbHelper.ScanTable<Category>();

        Assert.Single(returnedCategories);
        Assert.Equal(expectedCategoryName, returnedCategories[0].CategoryName);
        Assert.Equal(expectedSubcategories, returnedCategories[0].Subcategories);
        Assert.Equal(UserId, returnedCategories[0].UserId);
    }

    [Fact]
    public async Task
        GivenAnExpenseCategoryRequestForACategoryThatExists_WhenCreateCategoryIsCalled_ThenCategoryIsNotPersisted()
    {
        const string duplicateCategoryName = "category123";
        const string duplicateCategoryType = "expense";

        var initialData = new List<Category>
        {
            new()
            {
                UserId = UserId,
                CategoryName = $"expenseCategory#{duplicateCategoryName}",
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                UserId = UserId,
                CategoryName = "incomeCategory#category2",
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _dynamoDbHelper.WriteIntoTable(initialData);

        var inputDto = new CreateCategoryDto
        {
            CategoryName = duplicateCategoryName,
            CategoryType = duplicateCategoryType,
            Subcategories = new List<string> {"subcategory1", "subcategory2"}
        };

        var httpContent = new StringContent(JsonSerializer.Serialize(inputDto), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/categories", httpContent);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}