using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using TransactionService.Constants;
using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Domain.Models;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace TransactionService.IntegrationTests.TransactionsEndpoint;

[Collection("IntegrationTests")]
public class Feature_CockroachDb_TransactionsEndpointTests : IClassFixture<MoneyMateApiWebApplicationFactory>,
    IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;

    public Feature_CockroachDb_TransactionsEndpointTests(MoneyMateApiWebApplicationFactory factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>()
                {
                    ["CockroachDb:Enabled"] = "true"
                }))).CreateClient();
        _cockroachDbIntegrationTestHelper = new CockroachDbIntegrationTestHelper();
    }

    public async Task InitializeAsync()
    {
        await _cockroachDbIntegrationTestHelper.SeedRequiredData();
    }

    public async Task DisposeAsync()
    {
        await _cockroachDbIntegrationTestHelper.ClearDbData();
    }

    [Fact]
    public async Task GivenTransactionIdInput_WhenGetTransactionByIdIsCalled_ThenCorrectTransactionIsReturned()
    {
        var transaction = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = "income",
            Amount = 1223.45M,
            Category = "Test Category",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name2",
            Subcategory = "Salary"
        };

        await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(new List<Transaction> {transaction});

        var response = await _httpClient.GetAsync($"/api/transactions/{transaction.TransactionId}");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedTransaction = JsonSerializer.Deserialize<Transaction>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(transaction, returnedTransaction);
    }

    [Fact]
    public async Task GivenNoInputParameters_WhenGetTransactionsIsCalled_ThenAllTransactionsAreReturned()
    {
        var transaction1 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = "income",
            Amount = 1223.45M,
            Category = "Test Category",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name2",
            Subcategory = "Salary"
        };
        var transaction2 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
            TransactionTimestamp = new DateTime(2022, 02, 01, 0, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
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
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = new DateTime(2023, 02, 01, 0, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = "income",
            Amount = 123.45M,
            Category = "Salary",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name3",
            Subcategory = "Salary"
        };


        var transactionList = new List<Transaction>
        {
            transaction1,
            transaction2,
            transaction3
        };
        await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(transactionList);

        var response = await _httpClient.GetAsync("/api/transactions");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(transactionList, returnedTransactionList);
    }


    [Fact]
    public async Task GivenStartDateInputParameter_WhenGetTransactionsIsCalled_ThenCorrectTransactionsAreReturned()
    {
        var transaction1 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
            TransactionTimestamp = new DateTime(2021, 3, 28, 0, 0, 0, 0, DateTimeKind.Utc).ToString("O"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            Note = "this is a note123"
        };
        var transaction2 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = new DateTime(2022, 3, 28, 0, 0, 0, 0, DateTimeKind.Utc).ToString("O"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat"
        };

        var transaction3 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
            TransactionTimestamp = new DateTime(2020, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToString("O"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            Note = "this is a note123"
        };

        var transactionList = new List<Transaction>
        {
            transaction1,
            transaction2,
            transaction3
        };

        await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(transactionList);

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["start"] = new DateTime(2021, 3, 27, 0, 0, 0, DateTimeKind.Utc).ToString("O");
        var queryString = query.ToString();

        var response = await _httpClient.GetAsync($"/api/transactions?{queryString}");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        var expectedTransactionList = new List<Transaction>
        {
            transaction1, transaction2
        };
        Assert.Equal(expectedTransactionList, returnedTransactionList);
    }

    [Fact]
    public async Task GivenEndDateInputParameter_WhenGetTransactionsIsCalled_ThenCorrectTransactionsAreReturned()
    {
        var transaction1 = new Transaction()
        {
            TransactionTimestamp = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString("O"),
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            Note = "this is a note123"
        };
        var transaction2 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = new DateTime(2021, 2, 3, 0, 0, 0, DateTimeKind.Utc).ToString("O"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat"
        };
        var transaction3 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
            TransactionTimestamp = new DateTime(2021, 2, 5, 0, 0, 0, DateTimeKind.Utc).ToString("O"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat"
        };

        var transaction4 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915d",
            TransactionTimestamp = new DateTime(2021, 3, 5, 0, 0, 0, DateTimeKind.Utc).ToString("O"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            Note = "this is a note123"
        };

        var transactionList = new List<Transaction>
        {
            transaction1,
            transaction2,
            transaction3,
            transaction4
        };

        await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(transactionList);

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["end"] = new DateTime(2021, 3, 3).ToString("O");
        var queryString = query.ToString();

        var response = await _httpClient.GetAsync($"/api/transactions?{queryString}");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        returnedTransactionList!.Sort((transaction, transaction5) =>
            String.CompareOrdinal(transaction.TransactionId, transaction5.TransactionId));

        Assert.Equal(new List<Transaction>
        {
            transaction1, transaction2, transaction3
        }, returnedTransactionList);
    }


    [Fact]
    public async Task GivenTypeInputParameter_WhenGetTransactionsIsCalled_ThenAllTransactionsOfTypeAreReturned()
    {
        var transactionType = "expense";
        var transaction1 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = transactionType,
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name1",
            Note = "this is a note123"
        };
        var transaction2 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = "income",
            Amount = 123.45M,
            Category = "Income",
            Subcategory = "Salary",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name2"
        };

        var transactionList = new List<Transaction>
        {
            transaction1,
            transaction2
        };
        await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(transactionList);

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["type"] = transactionType;
        var queryString = query.ToString();

        var response = await _httpClient.GetAsync($"/api/transactions?{queryString}");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(new List<Transaction> {transaction1},
            returnedTransactionList);
    }

    [Fact]
    public async Task
        GivenPayerPayeeIdInputParameter_WhenGetTransactionsIsCalled_ThenAllTransactionsOfTypeAreReturned()
    {
        var transactionType = "expense";
        var transaction1 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = transactionType,
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name1",
            Note = "this is a note123"
        };
        var transaction2 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = "income",
            Amount = 123.45M,
            Category = "Income",
            Subcategory = "Salary"
        };

        var transactionList = new List<Transaction>
        {
            transaction1,
            transaction2
        };

        await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(transactionList);

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["payerPayeeId"] = "";
        var queryString = query.ToString();

        var response = await _httpClient.GetAsync($"/api/transactions?{queryString}");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(new List<Transaction> {transaction2}, returnedTransactionList);
    }

    [Fact]
    public async Task
        GivenCategoriesParameter_WhenGetTransactionsIsCalled_ThenAllTransactionsOfCategoryAreReturned()
    {
        var transactionType = "expense";
        var transaction1 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = transactionType,
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name1",
            Note = "this is a note123"
        };
        var transaction2 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name2",
            Subcategory = "Salary"
        };
        var transaction3 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = "income",
            Amount = 123.45M,
            Category = "Income",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name2",
            Subcategory = "Salary"
        };
        var transaction4 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915d",
            TransactionTimestamp = new DateTime(2020, 02, 01, 0, 0, 0, 0, DateTimeKind.Utc).ToString("o"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Eating Out",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name3",
            Subcategory = "Dinner"
        };

        var transactionList = new List<Transaction>
        {
            transaction1,
            transaction2,
            transaction3,
            transaction4
        };
        await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(transactionList);

        var queryString = "categories=Groceries&categories=Eating Out";

        var response = await _httpClient.GetAsync($"/api/transactions?{queryString}");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(
            new List<Transaction> {transaction1, transaction2, transaction4},
            returnedTransactionList);
    }

    [Fact]
    public async Task GivenInvalidRequest_WhenPostTransactionsIsCalled_ThenValidationErrorShouldBeReturned()
    {
        var inputDto = new StoreTransactionDto
        {
            Category = "Food",
            Subcategory = "Dinner",
            TransactionTimestamp = "2017-06-21T14:57:17",
            TransactionType = "expense",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name1",
            Note = "note123"
        };

        string inputDtoString = JsonSerializer.Serialize(inputDto);

        StringContent httpContent =
            new StringContent(inputDtoString, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"/api/transactions", httpContent);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Fact]
    public async Task
        GivenValidTransactionRequest_WhenPostTransactionsIsCalled_ThenCorrectTransactionsArePersisted()
    {
        const decimal expectedAmount = 123M;
        const string expectedCategory = "Food";
        const string expectedSubcategory = "Dinner";
        var expectedTransactionTimestamp =
            new DateTimeOffset(new DateTime(2021, 4, 2), TimeSpan.Zero).ToString("yyyy-MM-ddThh:mm:ss.FFFK");
        const string expectedTransactionType = "expense";
        var expectedPayerPayeeId = Guid.NewGuid().ToString();
        const string expectedPayerPayeeName = "name1";
        const string expectedNote = "this is a note123";
        var inputDto = new StoreTransactionDto
        {
            Amount = expectedAmount,
            Category = expectedCategory,
            Subcategory = expectedSubcategory,
            TransactionTimestamp = expectedTransactionTimestamp,
            TransactionType = expectedTransactionType,
            PayerPayeeId = expectedPayerPayeeId,
            PayerPayeeName = expectedPayerPayeeName,
            Note = expectedNote
        };

        await _cockroachDbIntegrationTestHelper.WriteCategoriesIntoDb(new List<Category>
        {
            new()
            {
                TransactionType = TransactionTypeExtensions.ConvertToTransactionType(expectedTransactionType),
                Subcategories = new List<string> {expectedSubcategory},
                CategoryName = expectedCategory
            }
        });

        await _cockroachDbIntegrationTestHelper.WritePayeesIntoDb(new List<PayerPayee>
        {
            new PayerPayee
            {
                PayerPayeeId = expectedPayerPayeeId,
                PayerPayeeName = expectedPayerPayeeName,
                ExternalId = "1234"
            }
        });

        string inputDtoString = JsonSerializer.Serialize(inputDto);

        StringContent httpContent =
            new StringContent(inputDtoString, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"/api/transactions", httpContent);
        response.EnsureSuccessStatusCode();

        var returnedTransactions = await _cockroachDbIntegrationTestHelper.GetAllTransactions();

        Assert.Single(returnedTransactions);
        Assert.Equal(expectedAmount, returnedTransactions[0].Amount);
        Assert.Equal(expectedCategory, returnedTransactions[0].Category);
        Assert.Equal(expectedSubcategory, returnedTransactions[0].Subcategory);
        // TODO: fix this once timestamp is datetimeoffset format
        // Assert.Equal(expectedTransactionTimestamp, returnedTransactions[0].TransactionTimestamp);
        Assert.Equal(expectedTransactionType, returnedTransactions[0].TransactionType);
        Assert.Equal(expectedPayerPayeeId, returnedTransactions[0].PayerPayeeId);
        Assert.Equal(expectedPayerPayeeName, returnedTransactions[0].PayerPayeeName);
        Assert.Equal(expectedNote, returnedTransactions[0].Note);
    }

    [Fact]
    public async Task GivenValidPutTransactionDto_WhenPutTransactionsIsCalled_ThenCorrectTransactionsArePersisted()
    {
        var expectedTransactionId = Guid.NewGuid().ToString();

        var transaction1 = new Transaction()
        {
            TransactionId = expectedTransactionId,
            TransactionTimestamp = DateTime.Now.ToString("o"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat",
            Note = "this is a note123"
        };
        var transaction2 = new Transaction
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = DateTime.Now.ToString("o"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat"
        };

        var transactionList = new List<Transaction>
        {
            transaction1,
            transaction2
        };

        await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(transactionList);

        const decimal expectedAmount = 123M;
        const string expectedCategory = "Food";
        const string expectedSubcategory = "Dinner";
        var expectedTransactionTimestamp =
            new DateTime(2021, 4, 2, 0, 0, 0, DateTimeKind.Utc).ToString("o");
        const string expectedTransactionType = "expense";
        const string expectedPayerPayeeId = "cc00567c-468e-4ccf-af4c-fca1c731915b";
        const string expectedPayerPayeeName = "name123";
        const string expectedNote = "This is a new note";

        await _cockroachDbIntegrationTestHelper.WriteCategoriesIntoDb(new List<Category>
        {
            new()
            {
                TransactionType = TransactionTypeExtensions.ConvertToTransactionType("expense"),
                Subcategories = new List<string> {expectedSubcategory},
                CategoryName = expectedCategory
            }
        });

        await _cockroachDbIntegrationTestHelper.WritePayeesIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = expectedPayerPayeeId,
                PayerPayeeName = expectedPayerPayeeName,
            }
        });

        var inputDto = new PutTransactionDto
        {
            Amount = expectedAmount,
            Category = expectedCategory,
            Subcategory = expectedSubcategory,
            TransactionTimestamp = expectedTransactionTimestamp,
            TransactionType = expectedTransactionType,
            PayerPayeeId = expectedPayerPayeeId,
            PayerPayeeName = expectedPayerPayeeName,
            Note = expectedNote
        };

        string inputDtoString = JsonSerializer.Serialize(inputDto);
        StringContent httpContent = new StringContent(inputDtoString, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"/api/transactions/{expectedTransactionId}", httpContent);
        response.EnsureSuccessStatusCode();


        var returned = await _cockroachDbIntegrationTestHelper.GetAllTransactions();

        var returnedTransaction = returned.Find(transaction => transaction.TransactionId == expectedTransactionId);

        var expectedTransaction = new Transaction()
        {
            TransactionId = expectedTransactionId,
            Amount = expectedAmount,
            Category = expectedCategory,
            Subcategory = expectedSubcategory,
            TransactionTimestamp = expectedTransactionTimestamp,
            TransactionType = expectedTransactionType,
            PayerPayeeId = expectedPayerPayeeId,
            PayerPayeeName = expectedPayerPayeeName,
            Note = expectedNote
        };
        Assert.Equal(expectedTransaction, returnedTransaction);
    }

    [Fact]
    public async Task GivenValidTransactionId_WhenDeleteTransactionIsCalled_ThenCorrectTransactionIsDeleted()
    {
        var expectedTransactionId = Guid.NewGuid().ToString();

        var transaction1 = new Transaction()
        {
            TransactionId = expectedTransactionId,
            TransactionTimestamp = DateTime.Now.ToString("o"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat"
        };
        var transaction2 = new Transaction()
        {
            TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
            TransactionTimestamp = DateTime.Now.ToString("o"),
            TransactionType = "expense",
            Amount = 123.45M,
            Category = "Groceries",
            Subcategory = "Meat"
        };

        var transactionList = new List<Transaction>
        {
            transaction1,
            transaction2
        };

        await _cockroachDbIntegrationTestHelper.WriteTransactionsIntoDb(transactionList);
        var response = await _httpClient.DeleteAsync($"/api/transactions/{expectedTransactionId}");
        response.EnsureSuccessStatusCode();

        var returnedTransaction = await _cockroachDbIntegrationTestHelper.GetTransactionById(expectedTransactionId);
        Assert.Null(returnedTransaction);
    }
}