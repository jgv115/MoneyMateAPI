using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TransactionService.Constants;
using TransactionService.Controllers.Categories.Dtos;
using TransactionService.Domain.Models;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using TransactionService.Repositories.DynamoDb.Models;
using Xunit;

namespace TransactionService.IntegrationTests.CategoriesEndpoint;

[Collection("IntegrationTests")]
public class CategoriesEndpointTests : IClassFixture<MoneyMateApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly DynamoDbHelper _dynamoDbHelper;
    private const string PersistedCategoriesUserId = "auth0|moneymatetest#Categories";
    private const string PersistedTransactionsUserId = "auth0|moneymatetest#Transaction";
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
                new List<DynamoDbCategory>
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
                    },
                    new()
                    {
                        UserId = ConsumerUserId,
                        CategoryName = "category3",
                        Subcategories = new List<string>()
                    }
                }
            },
            new object[]
            {
                "?transactionType=expense",
                new List<DynamoDbCategory>
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
                new List<DynamoDbCategory>
                {
                    new()
                    {
                        UserId = ConsumerUserId,
                        CategoryName = "category2",
                        Subcategories = new List<string> {"subcategory3", "subcategory4"}
                    },
                    new()
                    {
                        UserId = ConsumerUserId,
                        CategoryName = "category3",
                        Subcategories = new List<string>()
                    }
                }
            }
        };


    [Theory]
    [MemberData(nameof(CategoriesEndpointTestData))]
    public async Task GivenValidRequest_WhenGetCategoriesIsCalledWithCategoryType_ThenAllCategoriesAreReturned(
        string queryString, List<DynamoDbCategory> expectedCategories)
    {
        var initialData = new List<DynamoDbCategory>
        {
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = "category1",
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            },
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = "category3",
                TransactionType = TransactionType.Income,
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
            Assert.Equal(expectedCategories[i].Subcategories, returnedCategoriesList[i].Subcategories);
        }
    }

    [Fact]
    public async Task GivenValidRequest_WhenGetSubcategoriesIsCalled_ThenAllSubcategoriesAreReturned()
    {
        var inputCategories = new List<string> {"category1", "category2", "category3"};
        var expectedSubcategories = new List<string> {"test1", "test2", "test4"};

        await _dynamoDbHelper.WriteIntoTable(inputCategories.Select(category => new DynamoDbCategory
        {
            UserId = PersistedCategoriesUserId,
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

        var returnedCategories = await _dynamoDbHelper.ScanTable<DynamoDbCategory>();

        Assert.Collection(returnedCategories, category =>
        {
            Assert.Equal(inputCategoryName, category.CategoryName);
            Assert.Equal(expectedSubcategories, category.Subcategories);
            Assert.Equal(PersistedCategoriesUserId, category.UserId);
        });
    }

    [Fact]
    public async Task
        GivenAnCreateCategoryRequestForACategoryThatExists_WhenCreateCategoryIsCalled_ThenCategoryIsNotPersisted()
    {
        const string duplicateCategoryName = "category123";
        var duplicateTransactionType = TransactionType.Expense;

        var initialData = new List<DynamoDbCategory>
        {
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = duplicateCategoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                UserId = PersistedCategoriesUserId,
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
    public async Task GivenCategoryName_WhenDeleteCategoryIsCalled_ThenCategoryIsDeleted()
    {
        const string categoryName = "category123";

        var initialData = new List<DynamoDbCategory>
        {
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _dynamoDbHelper.WriteIntoTable(initialData);

        var response = await _httpClient.DeleteAsync($"api/categories/{categoryName}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var returnedCategory = await _dynamoDbHelper.QueryTable<DynamoDbCategory>(PersistedCategoriesUserId, categoryName);

        Assert.Null(returnedCategory);
    }

    [Fact]
    public async Task
        GivenAnAddCategoryPatchRequestForACategoryThatExists_WhenPatchCategoryIsCalled_ThenCategoryIsUpdated()
    {
        const string categoryName = "category123";

        var initialData = new List<DynamoDbCategory>
        {
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                UserId = PersistedCategoriesUserId,
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

        var returnedCategory = await _dynamoDbHelper.QueryTable<DynamoDbCategory>(PersistedCategoriesUserId, categoryName);

        Assert.Equal(new DynamoDbCategory
        {
            UserId = PersistedCategoriesUserId,
            CategoryName = categoryName,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"subcategory1", "subcategory2", "test subcategory"}
        }, returnedCategory);
    }

    [Fact]
    public async Task GivenAPatchCategoryRequestForACategoryThatDoesNotExist_WhenPatchCategoryIsCalled_Then404Returned()
    {
        var inputPatchDoc = "[{ \"op\": \"add\", \"path\": \"/subcategories/-\", \"value\": \"test subcategory\" }]";

        var httpContent = new StringContent(inputPatchDoc,
            Encoding.UTF8, "application/json-patch+json");
        var response = await _httpClient.PatchAsync($"/api/categories/categoryname", httpContent);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task
        GivenDeleteSubcategoryPatchRequest_ThenSubcategoryIsDeleted()
    {
        const string categoryName = "category123";

        var initialData = new List<DynamoDbCategory>
        {
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _dynamoDbHelper.WriteIntoTable(initialData);


        var inputPatchDoc = "[{ \"op\": \"remove\", \"path\": \"/subcategories/0\"}]";

        var httpContent = new StringContent(inputPatchDoc,
            Encoding.UTF8, "application/json-patch+json");
        var response = await _httpClient.PatchAsync($"/api/categories/{categoryName}", httpContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var returnedCategory = await _dynamoDbHelper.QueryTable<DynamoDbCategory>(PersistedCategoriesUserId, categoryName);

        Assert.Equal(new DynamoDbCategory
        {
            UserId = PersistedCategoriesUserId,
            CategoryName = categoryName,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"subcategory2"}
        }, returnedCategory);
    }

    [Fact]
    public async Task
        GivenUpdateSubcategoryNamePatchRequest_ThenSubcategoryIsRenamedAndTransactionsAreModified()
    {
        const string categoryName = "category123";
        const string existingSubcategoryName = "subcategory1";

        var initialCategories = new List<DynamoDbCategory>
        {
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {existingSubcategoryName, "subcategory2"}
            },
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _dynamoDbHelper.WriteIntoTable(initialCategories);

        var transaction1 = new DynamoDbTransaction
        {
            UserId = PersistedTransactionsUserId,
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
            TransactionTimestamp = new DateTime(2020, 02, 01).ToString("o"),
            TransactionType = "income",
            Amount = 1223.45M,
            Category = categoryName,
            Subcategory = existingSubcategoryName,
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name2",
        };
        var transaction2 = new DynamoDbTransaction
        {
            UserId = PersistedTransactionsUserId,
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = DateTime.Now.ToString("o"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name1",
            Note = "this is a note123"
        };
        var transaction3 = new DynamoDbTransaction
        {
            UserId = PersistedTransactionsUserId,
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
            TransactionTimestamp = DateTime.Now.ToString("o"),
            TransactionType = "income",
            Amount = 123.45M,
            Category = categoryName,
            Subcategory = existingSubcategoryName,
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name2",
        };

        var transactionList = new List<DynamoDbTransaction>
        {
            transaction1,
            transaction2,
            transaction3
        };

        await _dynamoDbHelper.WriteTransactionsIntoTable(transactionList);

        var inputPatchDoc =
            "[{ \"op\": \"replace\", \"path\": \"/subcategories/0\", \"value\": \"renamed subcategory\"}]";

        var httpContent = new StringContent(inputPatchDoc,
            Encoding.UTF8, "application/json-patch+json");
        var response = await _httpClient.PatchAsync($"/api/categories/{categoryName}", httpContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var returnedCategory = await _dynamoDbHelper.QueryTable<DynamoDbCategory>(PersistedCategoriesUserId, categoryName);

        Assert.Equal(new DynamoDbCategory
        {
            UserId = PersistedCategoriesUserId,
            CategoryName = categoryName,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"renamed subcategory", "subcategory2"}
        }, returnedCategory);

        transaction1.Subcategory = "renamed subcategory";
        transaction3.Subcategory = "renamed subcategory";
        var returnedTransactions = await _dynamoDbHelper.QueryTable<DynamoDbTransaction>(PersistedTransactionsUserId);

        Assert.Equal(transactionList, returnedTransactions);
    }

    [Fact]
    public async Task
        GivenUpdateCategoryNamePatchRequest_ThenCategoryIsRenamedAndTransactionsAreModified()
    {
        const string categoryName = "category123";

        var initialCategories = new List<DynamoDbCategory>
        {
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                UserId = PersistedCategoriesUserId,
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _dynamoDbHelper.WriteIntoTable(initialCategories);

        var transaction1 = new DynamoDbTransaction
        {
            UserId = PersistedTransactionsUserId,
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
            TransactionTimestamp = new DateTime(2020, 02, 01).ToString("o"),
            TransactionType = "income",
            Amount = 1223.45M,
            Category = categoryName,
            Subcategory = "subcategory1",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name2",
        };
        var transaction2 = new DynamoDbTransaction
        {
            UserId = PersistedTransactionsUserId,
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = DateTime.Now.ToString("o"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name1",
            Note = "this is a note123"
        };
        var transaction3 = new DynamoDbTransaction
        {
            UserId = PersistedTransactionsUserId,
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
            TransactionTimestamp = DateTime.Now.ToString("o"),
            TransactionType = "income",
            Amount = 123.45M,
            Category = categoryName,
            Subcategory = "subcategory1",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name2",
        };

        var transactionList = new List<DynamoDbTransaction>
        {
            transaction1,
            transaction2,
            transaction3
        };

        await _dynamoDbHelper.WriteTransactionsIntoTable(transactionList);

        var inputPatchDoc =
            "[{ \"op\": \"replace\", \"path\": \"/categoryName\", \"value\": \"renamed category\"}]";

        var httpContent = new StringContent(inputPatchDoc,
            Encoding.UTF8, "application/json-patch+json");
        var response = await _httpClient.PatchAsync($"/api/categories/{categoryName}", httpContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Test that new category exists
        var returnedCategory =
            await _dynamoDbHelper.QueryTable<DynamoDbCategory>(PersistedCategoriesUserId, "renamed category");

        Assert.Equal(new DynamoDbCategory
        {
            UserId = PersistedCategoriesUserId,
            CategoryName = "renamed category",
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"subcategory1", "subcategory2"}
        }, returnedCategory);
        
        // Test that old category does not exist
        var oldCategory = await _dynamoDbHelper.QueryTable<DynamoDbCategory>(PersistedCategoriesUserId, categoryName);
        Assert.Null(oldCategory);

        transaction1.Category = "renamed category";
        transaction3.Category = "renamed category";
        var returnedTransactions = await _dynamoDbHelper.QueryTable<DynamoDbTransaction>(PersistedTransactionsUserId);

        Assert.Equal(transactionList, returnedTransactions);
    }
}