using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.IntegrationTests.Extensions;
using MoneyMateApi.IntegrationTests.Helpers;
using MoneyMateApi.IntegrationTests.WebApplicationFactories;
using MoneyMateApi.Tests.Common;
using Xunit;

namespace MoneyMateApi.IntegrationTests.TransactionsEndpoint;

[Collection("IntegrationTests")]
public class TransactionsEndpointTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;

    public TransactionsEndpointTests(MoneyMateApiWebApplicationFactory factory)
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

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(new List<Transaction>
            { transaction });

        var response = await _httpClient.GetAsync($"/api/transactions/{transaction.TransactionId}");
        await response.AssertSuccessfulStatusCode();

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
        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

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

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

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

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

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

        Assert.Equal([transaction1, transaction2, transaction3], returnedTransactionList);
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
        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

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

        Assert.Equal(new List<Transaction> { transaction1 },
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

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

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

        Assert.Equal(new List<Transaction> { transaction2 }, returnedTransactionList);
    }

    [Fact]
    public async Task
        GivenCategoriesParameter_WhenGetTransactionsIsCalled_ThenAllTransactionsOfCategoryAreReturned()
    {
        var transactionListBuilder = new TransactionListBuilder()
            .WithTransactions(1, Guid.NewGuid().ToString(), "name1", 123.45M, TransactionType.Expense, "Groceries",
                "Meat", "this is a note123")
            .WithTransactions(1, Guid.NewGuid().ToString(), "name2", 123.45M, TransactionType.Expense, "Groceries",
                "vegetables", tagIds: [Guid.NewGuid()])
            .WithTransactions(1, Guid.NewGuid().ToString(), "name2", 123.45M, TransactionType.Income, "Income",
                "Salary")
            .WithTransactions(1, Guid.NewGuid().ToString(), "name3", 123.45M, TransactionType.Expense, "Eating Out",
                "Dinner", tagIds: [Guid.NewGuid()]);

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionListBuilder
            .BuildDomainModels());

        var queryString = "categories=Groceries&categories=Eating Out";

        var response = await _httpClient.GetAsync($"/api/transactions?{queryString}");
        await response.AssertSuccessfulStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedTransactionList = JsonSerializer.Deserialize<List<TransactionOutputDto>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        var transactionDtos = transactionListBuilder.BuildOutputDtos();
        Assert.Equal(
            [transactionDtos[0], transactionDtos[1], transactionDtos[3]],
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

        await _cockroachDbIntegrationTestHelper.CategoryOperations.WriteCategoriesIntoDb(new List<Category>
        {
            new()
            {
                TransactionType = TransactionTypeExtensions.ConvertToTransactionType(expectedTransactionType),
                Subcategories = new List<string> { expectedSubcategory },
                CategoryName = expectedCategory
            }
        });

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayeesIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = expectedPayerPayeeId,
                PayerPayeeName = expectedPayerPayeeName,
                ExternalId = "1234"
            }
        });

        string inputDtoString = JsonSerializer.Serialize(inputDto);

        var httpContent =
            new StringContent(inputDtoString, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"/api/transactions", httpContent);
        response.EnsureSuccessStatusCode();

        var returnedTransactions = await _cockroachDbIntegrationTestHelper.TransactionOperations.GetAllTransactions();

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
        Assert.Equal(new List<Guid>(), returnedTransactions[0].TagIds);
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

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        const decimal expectedAmount = 123M;
        const string expectedCategory = "Food";
        const string expectedSubcategory = "Dinner";
        var expectedTransactionTimestamp =
            new DateTime(2021, 4, 2, 0, 0, 0, DateTimeKind.Utc).ToString("o");
        const string expectedTransactionType = "expense";
        const string expectedPayerPayeeId = "cc00567c-468e-4ccf-af4c-fca1c731915b";
        const string expectedPayerPayeeName = "name123";
        const string expectedNote = "This is a new note";

        await _cockroachDbIntegrationTestHelper.CategoryOperations.WriteCategoriesIntoDb([
            new()
            {
                TransactionType = TransactionTypeExtensions.ConvertToTransactionType("expense"),
                Subcategories = new List<string> { expectedSubcategory },
                CategoryName = expectedCategory
            }
        ]);

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayeesIntoDb([
            new()
            {
                PayerPayeeId = expectedPayerPayeeId,
                PayerPayeeName = expectedPayerPayeeName,
            }
        ]);

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

        var inputDtoString = JsonSerializer.Serialize(inputDto);
        var httpContent = new StringContent(inputDtoString, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"/api/transactions/{expectedTransactionId}", httpContent);
        response.EnsureSuccessStatusCode();


        var returned = await _cockroachDbIntegrationTestHelper.TransactionOperations.GetAllTransactions();

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

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);
        var response = await _httpClient.DeleteAsync($"/api/transactions/{expectedTransactionId}");
        response.EnsureSuccessStatusCode();

        var returnedTransaction =
            await _cockroachDbIntegrationTestHelper.TransactionOperations.GetTransactionById(expectedTransactionId);
        Assert.Null(returnedTransaction);
    }
}