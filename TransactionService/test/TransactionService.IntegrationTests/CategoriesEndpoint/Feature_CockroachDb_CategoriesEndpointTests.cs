using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using TransactionService.Constants;
using TransactionService.Controllers.Categories.Dtos;
using TransactionService.Domain.Models;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace TransactionService.IntegrationTests.CategoriesEndpoint;

[Collection("IntegrationTests")]
public class Feature_CockroachDb_CategoriesEndpointTests : IClassFixture<MoneyMateApiWebApplicationFactory>,
    IAsyncLifetime
{
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly DynamoDbHelper _dynamoDbHelper;

    public Feature_CockroachDb_CategoriesEndpointTests(MoneyMateApiWebApplicationFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((context, configurationBuilder) =>
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["CockroachDb:Enabled"] = "true"
                })));
        _cockroachDbIntegrationTestHelper = new CockroachDbIntegrationTestHelper();
        _dynamoDbHelper = factory.DynamoDbHelper;
    }

    public async Task InitializeAsync()
    {
        await _cockroachDbIntegrationTestHelper.SeedRequiredData();
        // TODO: this needs to stay here until we have a CockroachDBTransactionRepository
        await _dynamoDbHelper.CreateTable();
    }

    public async Task DisposeAsync()
    {
        await _cockroachDbIntegrationTestHelper.ClearDbData();
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

        var response = await _factory.CreateDefaultClient().GetAsync($"/api/categories{queryString}");
        var returnedString = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var returnedCategoriesList = JsonSerializer.Deserialize<List<Category>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        for (var i = 0; i < expectedCategories.Count; i++)
        {
            Assert.Equal(expectedCategories[i], returnedCategoriesList[i]);
            // Assert.Equal(expectedCategories[i].CategoryName, returnedCategoriesList![i].CategoryName);
            // Assert.Equal(expectedCategories[i].Subcategories, returnedCategoriesList[i].Subcategories);
        }
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

        var response = await _factory.CreateDefaultClient().GetAsync("/api/categories/category1");
        var returnedString = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

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
        var response = await _factory.CreateDefaultClient().PostAsync("/api/categories", httpContent);

        response.EnsureSuccessStatusCode();

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
        var response = await _factory.CreateDefaultClient().PostAsync("/api/categories", httpContent);

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

        var response = await _factory.CreateDefaultClient().DeleteAsync($"api/categories/{categoryName}");

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
        var response = await _factory.CreateDefaultClient().PatchAsync($"/api/categories/{categoryName}", httpContent);

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
        var response = await _factory.CreateDefaultClient().PatchAsync($"/api/categories/categoryname", httpContent);

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
        var response = await _factory.CreateDefaultClient().PatchAsync($"/api/categories/{categoryName}", httpContent);

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
    
    // TODO: the rest of these tests need to be completed when we are able to insert transactions
    // [Fact]
    // public async Task
    //     GivenUpdateSubcategoryNamePatchRequest_ThenSubcategoryIsRenamedAndTransactionsAreModified()
    // {
    //     const string categoryName = "category123";
    //     const string existingSubcategoryName = "subcategory1";
    //
    //     var initialCategories = new List<Category>
    //     {
    //         new()
    //         {
    //             CategoryName = categoryName,
    //             TransactionType = TransactionType.Expense,
    //             Subcategories = new List<string> {existingSubcategoryName, "subcategory2"}
    //         },
    //         new()
    //         {
    //             CategoryName = "category2",
    //             TransactionType = TransactionType.Income,
    //             Subcategories = new List<string> {"subcategory3", "subcategory4"}
    //         }
    //     };
    //
    //     await _cockroachDbIntegrationTestHelper.WriteCategoriesIntoDb(initialCategories);
    //
    //     var transaction1 = new Transaction()
    //     {
    //         TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
    //         TransactionTimestamp = new DateTime(2020, 02, 01).ToString("o"),
    //         TransactionType = "income",
    //         Amount = 1223.45M,
    //         Category = categoryName,
    //         Subcategory = existingSubcategoryName,
    //         PayerPayeeId = Guid.NewGuid().ToString(),
    //         PayerPayeeName = "name2",
    //     };
    //     var transaction2 = new Transaction()
    //     {
    //         TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
    //         TransactionTimestamp = DateTime.Now.ToString("o"),
    //         TransactionType = "expense",
    //         Amount = 123.45M,
    //         Category = "Groceries",
    //         Subcategory = "Meat",
    //         PayerPayeeId = Guid.NewGuid().ToString(),
    //         PayerPayeeName = "name1",
    //         Note = "this is a note123"
    //     };
    //     var transaction3 = new Transaction()
    //     {
    //         TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
    //         TransactionTimestamp = DateTime.Now.ToString("o"),
    //         TransactionType = "income",
    //         Amount = 123.45M,
    //         Category = categoryName,
    //         Subcategory = existingSubcategoryName,
    //         PayerPayeeId = Guid.NewGuid().ToString(),
    //         PayerPayeeName = "name2",
    //     };
    //
    //     var transactionList = new List<Transaction>
    //     {
    //         transaction1,
    //         transaction2,
    //         transaction3
    //     };
    //
    //     await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(transactionList);
    //
    //     var inputPatchDoc =
    //         "[{ \"op\": \"replace\", \"path\": \"/subcategories/0\", \"value\": \"renamed subcategory\"}]";
    //
    //     var httpContent = new StringContent(inputPatchDoc,
    //         Encoding.UTF8, "application/json-patch+json");
    //     var response = await _httpClient.PatchAsync($"/api/categories/{categoryName}", httpContent);
    //
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //
    //     var returnedCategory = await _dynamoDbHelper.QueryTable<DynamoDbCategory>(PersistedCategoriesUserId, categoryName);
    //
    //     Assert.Equal(new DynamoDbCategory
    //     {
    //         UserId = PersistedCategoriesUserId,
    //         CategoryName = categoryName,
    //         TransactionType = TransactionType.Expense,
    //         Subcategories = new List<string> {"renamed subcategory", "subcategory2"}
    //     }, returnedCategory);
    //
    //     transaction1.Subcategory = "renamed subcategory";
    //     transaction3.Subcategory = "renamed subcategory";
    //     var returnedTransactions = await _dynamoDbHelper.QueryTable<DynamoDbTransaction>(PersistedTransactionsUserId);
    //
    //     Assert.Equal(transactionList, returnedTransactions);
    // }

}