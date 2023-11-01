using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TransactionService.Constants;
using TransactionService.Controllers.Categories.Dtos;
using TransactionService.Domain.Models;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace TransactionService.IntegrationTests.CategoriesEndpoint;

[Collection("IntegrationTests")]
public class Feature_CockroachDb_CategoriesEndpointTests : IClassFixture<MoneyMateApiWebApplicationFactory>,
    IAsyncLifetime
{
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly HttpClient _httpClient;

    public Feature_CockroachDb_CategoriesEndpointTests(MoneyMateApiWebApplicationFactory factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>()
                {
                    ["CockroachDb:Enabled"] = "true"
                }))).CreateClient();
        _cockroachDbIntegrationTestHelper = factory.CockroachDbIntegrationTestHelper;
    }

    public async Task InitializeAsync()
    {
        await _cockroachDbIntegrationTestHelper.SeedRequiredData();
    }

    public async Task DisposeAsync()
    {
        await _cockroachDbIntegrationTestHelper.ClearDbData();
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
                        CategoryName = "category1",
                        Subcategories = new List<string> {"subcategory1", "subcategory2"},
                        TransactionType = TransactionType.Expense
                    },
                    new()
                    {
                        CategoryName = "category2",
                        Subcategories = new List<string> {"subcategory3", "subcategory4"},
                        TransactionType = TransactionType.Income
                    },
                    new()
                    {
                        CategoryName = "category3",
                        Subcategories = new List<string>(),
                        TransactionType = TransactionType.Income
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
                        CategoryName = "category1",
                        Subcategories = new List<string> {"subcategory1", "subcategory2"},
                        TransactionType = TransactionType.Expense
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
                        CategoryName = "category2",
                        Subcategories = new List<string> {"subcategory3", "subcategory4"},
                        TransactionType = TransactionType.Income
                    },
                    new()
                    {
                        CategoryName = "category3",
                        Subcategories = new List<string>(),
                        TransactionType = TransactionType.Income
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
                CategoryName = "category1",
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            },
            new()
            {
                CategoryName = "category3",
                TransactionType = TransactionType.Income,
            }
        };

        await _cockroachDbIntegrationTestHelper.WriteCategoriesIntoDb(initialData);

        var response = await _httpClient.GetAsync($"/api/categories{queryString}");
        var returnedString = await response.Content.ReadAsStringAsync();

        await response.AssertSuccessfulStatusCode();

        var returnedCategoriesList = JsonSerializer.Deserialize<List<Category>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(expectedCategories, returnedCategoriesList);
    }

    [Fact]
    public async Task GivenValidRequest_WhenGetSubcategoriesIsCalled_ThenAllSubcategoriesAreReturned()
    {
        var inputCategories = new List<string> {"category1", "category2", "category3"};
        var expectedSubcategories = new List<string> {"test1", "test2", "test4"};

        await _cockroachDbIntegrationTestHelper.WriteCategoriesIntoDb(inputCategories.Select(category => new Category()
        {
            CategoryName = category,
            TransactionType = TransactionType.Expense,
            Subcategories = expectedSubcategories
        }).ToList());

        var response = await _httpClient.GetAsync("/api/categories/category1");
        var returnedString = await response.Content.ReadAsStringAsync();

        await response.AssertSuccessfulStatusCode();

        var returnedSubcategoriesList = JsonSerializer.Deserialize<List<string>>(returnedString);

        expectedSubcategories.Sort();
        returnedSubcategoriesList.Sort();
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

        await response.AssertSuccessfulStatusCode();

        var returnedCategories = await _cockroachDbIntegrationTestHelper.RetrieveAllCategories();

        Assert.Collection(returnedCategories, category =>
        {
            Assert.Equal(inputCategoryName, category.CategoryName);
            expectedSubcategories.Sort();
            category.Subcategories.Sort();
            Assert.Equal(expectedSubcategories, category.Subcategories);
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
                CategoryName = duplicateCategoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _cockroachDbIntegrationTestHelper.WriteCategoriesIntoDb(initialData);

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

        var initialData = new List<Category>
        {
            new()
            {
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _cockroachDbIntegrationTestHelper.WriteCategoriesIntoDb(initialData);

        var response = await _httpClient.DeleteAsync($"api/categories/{categoryName}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var returnedCategories = await _cockroachDbIntegrationTestHelper.RetrieveAllCategories();

        Assert.DoesNotContain(returnedCategories, category => category.CategoryName == categoryName);
    }


    [Fact]
    public async Task
        GivenAnAddCategoryPatchRequestForACategoryThatExists_WhenPatchCategoryIsCalled_ThenCategoryIsUpdated()
    {
        const string categoryName = "category123";

        var initialData = new List<Category>
        {
            new()
            {
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _cockroachDbIntegrationTestHelper.WriteCategoriesIntoDb(initialData);


        var inputPatchDoc = "[{ \"op\": \"add\", \"path\": \"/subcategories/-\", \"value\": \"test subcategory\" }]";

        var httpContent = new StringContent(inputPatchDoc,
            Encoding.UTF8, "application/json-patch+json");
        var response = await _httpClient.PatchAsync($"/api/categories/{categoryName}", httpContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var returnedCategories = await _cockroachDbIntegrationTestHelper.RetrieveAllCategories();

        var modifiedCategory = returnedCategories.Find(category => category.CategoryName == categoryName);

        Assert.Equal(new Category()
        {
            CategoryName = categoryName,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"subcategory1", "subcategory2", "test subcategory"}
        }, modifiedCategory);
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

        var initialData = new List<Category>
        {
            new()
            {
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _cockroachDbIntegrationTestHelper.WriteCategoriesIntoDb(initialData);


        var inputPatchDoc = "[{ \"op\": \"remove\", \"path\": \"/subcategories/0\"}]";

        var httpContent = new StringContent(inputPatchDoc,
            Encoding.UTF8, "application/json-patch+json");
        var response = await _httpClient.PatchAsync($"/api/categories/{categoryName}", httpContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var returnedCategories = await _cockroachDbIntegrationTestHelper.RetrieveAllCategories();
        var modifiedCategory = returnedCategories.Find(category => category.CategoryName == categoryName);
        Assert.Equal(new Category()
        {
            CategoryName = categoryName,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"subcategory2"}
        }, modifiedCategory);
    }

    [Fact]
    public async Task
        GivenUpdateSubcategoryNamePatchRequest_ThenSubcategoryIsRenamedAndTransactionsAreModified()
    {
        const string categoryName = "category123";
        const string existingSubcategoryName = "subcategory1";

        var initialCategories = new List<Category>
        {
            new()
            {
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {existingSubcategoryName, "subcategory2"}
            },
            new()
            {
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _cockroachDbIntegrationTestHelper.WriteCategoriesIntoDb(initialCategories);

        var transaction1 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = "income",
            Amount = 1223.45M,
            Category = categoryName,
            Subcategory = existingSubcategoryName,
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name2",
        };
        var transaction2 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 1, DateTimeKind.Utc).ToString("o"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name1",
            Note = "this is a note123"
        };
        var transaction3 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 2, DateTimeKind.Utc).ToString("o"),
            TransactionType = "income",
            Amount = 123.45M,
            Category = categoryName,
            Subcategory = existingSubcategoryName,
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name3",
        };

        var transactionList = new List<Transaction>
        {
            transaction1,
            transaction2,
            transaction3
        };

        await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(transactionList);

        var inputPatchDoc =
            "[{ \"op\": \"replace\", \"path\": \"/subcategories/0\", \"value\": \"renamed subcategory\"}]";

        var httpContent = new StringContent(inputPatchDoc,
            Encoding.UTF8, "application/json-patch+json");
        var response = await _httpClient.PatchAsync($"/api/categories/{categoryName}", httpContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var returnedCategory = await _cockroachDbIntegrationTestHelper.RetrieveCategory(categoryName);
        Assert.Equal(new Category
        {
            CategoryName = categoryName,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"renamed subcategory", "subcategory2"}
        }, returnedCategory);

        transaction1.Subcategory = "renamed subcategory";
        transaction3.Subcategory = "renamed subcategory";
        var returnedTransactions = await _cockroachDbIntegrationTestHelper.GetAllTransactions();

        Assert.Equal(transactionList, returnedTransactions);
    }

    [Fact]
    public async Task
        GivenUpdateCategoryNamePatchRequest_ThenCategoryIsRenamedAndTransactionsAreModified()
    {
        const string categoryName = "category123";

        var initialCategories = new List<Category>
        {
            new()
            {
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        await _cockroachDbIntegrationTestHelper.WriteCategoriesIntoDb(initialCategories);

        var transaction1 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, DateTimeKind.Utc).ToString("O"),
            TransactionType = "income",
            Amount = 1223.45M,
            Category = categoryName,
            Subcategory = "subcategory1",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name2",
        };
        var transaction2 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 1, DateTimeKind.Utc).ToString("O"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name1",
            Note = "this is a note123"
        };
        var transaction3 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 2, DateTimeKind.Utc).ToString("O"),
            TransactionType = "income",
            Amount = 123.45M,
            Category = categoryName,
            Subcategory = "subcategory1",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name3",
        };

        var transactionList = new List<Transaction>
        {
            transaction1,
            transaction2,
            transaction3
        };

        await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(transactionList);

        var inputPatchDoc =
            "[{ \"op\": \"replace\", \"path\": \"/categoryName\", \"value\": \"renamed category\"}]";

        var httpContent = new StringContent(inputPatchDoc,
            Encoding.UTF8, "application/json-patch+json");
        var response = await _httpClient.PatchAsync($"/api/categories/{categoryName}", httpContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Test that new category exists
        var returnedCategory = await _cockroachDbIntegrationTestHelper.RetrieveCategory("renamed category");

        Assert.Equal(new Category()
        {
            CategoryName = "renamed category",
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"subcategory1", "subcategory2"}
        }, returnedCategory);

        // Test that old category does not exist
        var oldCategory = await _cockroachDbIntegrationTestHelper.RetrieveCategory(categoryName);
        Assert.Null(oldCategory);

        transaction1.Category = "renamed category";
        transaction3.Category = "renamed category";
        var returnedTransactions = await _cockroachDbIntegrationTestHelper.GetAllTransactions();

        Assert.Equal(transactionList, returnedTransactions);
    }
}