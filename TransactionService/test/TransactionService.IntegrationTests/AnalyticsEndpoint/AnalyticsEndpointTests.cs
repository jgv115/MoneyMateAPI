using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using TransactionService.Controllers.Analytics.ViewModels;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using TransactionService.Tests.Common;
using Xunit;

namespace TransactionService.IntegrationTests.AnalyticsEndpoint;

[Collection("IntegrationTests")]
public class AnalyticsEndpointTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;

    public AnalyticsEndpointTests(MoneyMateApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateDefaultClient();
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
    public async Task
        GivenStartAndEndInputParameters_WhenGetCategoriesBreakdownEndpointInvoked_ThenCorrectCategoriesReturned()
    {
        const string expectedCategory1 = "category1";
        const int numberOfCategory1Transactions = 10;
        const decimal category1TransactionsAmount = (decimal) 34.5;

        const string expectedCategory2 = "category2";
        const int numberOfCategory2Transactions = 5;
        const decimal category2TransactionsAmount = (decimal) 34.5;

        const string expectedCategory3 = "category3";
        const int numberOfCategory3Transactions = 2;
        const decimal category3TransactionsAmount = (decimal) 34.5;

        var transactionList = new TransactionListBuilder()
            .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory1Transactions, expectedCategory1,
                category1TransactionsAmount)
            .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory2Transactions, expectedCategory2,
                category2TransactionsAmount)
            .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory3Transactions, expectedCategory3,
                category3TransactionsAmount)
            .Build();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["type"] = "expense";
        query["start"] = new DateTime(2021, 4, 1).ToString("yyyy-MM-dd");
        query["end"] = new DateTime(2099, 4, 1).ToString("yyyy-MM-dd");
        var queryString = query.ToString();

        var response = await _httpClient.GetAsync($"/api/analytics/categories?{queryString}");

        await response.AssertSuccessfulStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var actualAnalyticsCategories = JsonSerializer.Deserialize<List<AnalyticsCategory>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        var expectedAnalyticsCategories = new List<AnalyticsCategory>
        {
            new()
            {
                CategoryName = expectedCategory1,
                TotalAmount = numberOfCategory1Transactions * category1TransactionsAmount
            },
            new()
            {
                CategoryName = expectedCategory2,
                TotalAmount = numberOfCategory2Transactions * category2TransactionsAmount
            },
            new()
            {
                CategoryName = expectedCategory3,
                TotalAmount = numberOfCategory3Transactions * category3TransactionsAmount
            }
        };
        Assert.Equal(expectedAnalyticsCategories, actualAnalyticsCategories);
    }

    [Fact]
    public async Task
        GivenFrequencyAndPeriodInputParameters_WhenGetCategoriesBreakdownEndpointInvoked_ThenCorrectCategoriesReturned()
    {
        const string expectedCategory1 = "category1";
        const int numberOfCategory1Transactions = 10;
        const decimal category1TransactionsAmount = (decimal) 34.5;

        const string expectedCategory2 = "category2";
        const int numberOfCategory2Transactions = 5;
        const decimal category2TransactionsAmount = (decimal) 34.5;

        const string expectedCategory3 = "category3";
        const int numberOfCategory3Transactions = 2;
        const decimal category3TransactionsAmount = (decimal) 34.5;

        var transactionList = new TransactionListBuilder()
            .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory1Transactions, expectedCategory1,
                category1TransactionsAmount)
            .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory2Transactions, expectedCategory2,
                category2TransactionsAmount)
            .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory3Transactions, expectedCategory3,
                category3TransactionsAmount)
            .Build();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["type"] = "expense";
        query["frequency"] = "MONTH";
        query["periods"] = "0";
        var queryString = query.ToString();

        var response = await _httpClient.GetAsync($"/api/analytics/categories?{queryString}");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var actualAnalyticsCategories = JsonSerializer.Deserialize<List<AnalyticsCategory>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        var expectedAnalyticsCategories = new List<AnalyticsCategory>
        {
            new()
            {
                CategoryName = expectedCategory1,
                TotalAmount = numberOfCategory1Transactions * category1TransactionsAmount
            },
            new()
            {
                CategoryName = expectedCategory2,
                TotalAmount = numberOfCategory2Transactions * category2TransactionsAmount
            },
            new()
            {
                CategoryName = expectedCategory3,
                TotalAmount = numberOfCategory3Transactions * category3TransactionsAmount
            }
        };
        Assert.Equal(expectedAnalyticsCategories, actualAnalyticsCategories);
    }

    [Fact]
    public async Task
        GivenStartAndEndInputParameters_WhenGetSubcategoriesBreakdownEndpointInvoked_ThenCorrectSubcategoriesReturned()
    {
        const string expectedCategory1 = "category1";
        const string expectedSubcategory1 = "subcategory1";
        const int numberOfSubcategory1Transactions = 10;
        const decimal subcategory1TransactionsAmount = (decimal) 34.5;

        const string expectedCategory2 = "category2";
        const string expectedSubcategory2 = "subcategory2";
        const int numberOfSubcategory2Transactions = 5;
        const decimal subcategory2TransactionsAmount = (decimal) 34.5;

        const string expectedCategory3 = "category3";
        const string expectedSubcategory3 = "subcategory3";
        const int numberOfSubcategory3Transactions = 2;
        const decimal subcategory3TransactionsAmount = (decimal) 34.5;

        const string expectedSubcategory4 = "subcategory4";
        const int numberOfSubcategory4Transactions = 2;
        const decimal subcategory4TransactionsAmount = (decimal) 12.4;

        var transactionList = new TransactionListBuilder()
            .WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(numberOfSubcategory1Transactions,
                expectedCategory1, expectedSubcategory1, subcategory1TransactionsAmount)
            .WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(numberOfSubcategory2Transactions,
                expectedCategory2,
                expectedSubcategory2, subcategory2TransactionsAmount)
            .WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(numberOfSubcategory3Transactions,
                expectedCategory3, expectedSubcategory3, subcategory3TransactionsAmount)
            .WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(numberOfSubcategory4Transactions,
                expectedCategory3, expectedSubcategory4, subcategory4TransactionsAmount)
            .Build();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["category"] = expectedCategory3;
        query["start"] = new DateTime(2021, 4, 1).ToString("yyyy-MM-dd");
        query["end"] = new DateTime(2099, 4, 1).ToString("yyyy-MM-dd");
        var queryString = query.ToString();

        var response = await _httpClient.GetAsync($"/api/analytics/subcategories?{queryString}");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var actualAnalyticsCategories = JsonSerializer.Deserialize<List<AnalyticsSubcategory>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        var expectedAnalyticsSubcategories = new List<AnalyticsSubcategory>
        {
            new()
            {
                SubcategoryName = expectedSubcategory3,
                BelongsToCategory = expectedCategory3,
                TotalAmount = numberOfSubcategory3Transactions * subcategory3TransactionsAmount
            },
            new()
            {
                SubcategoryName = expectedSubcategory4,
                BelongsToCategory = expectedCategory3,
                TotalAmount = numberOfSubcategory4Transactions * subcategory4TransactionsAmount
            }
        };
        Assert.Equal(expectedAnalyticsSubcategories, actualAnalyticsCategories);
    }

    [Fact]
    public async Task
        GivenStartAndEndInputParameters_WhenGetPayerPayeeBreakdownEndpointInvoked_ThenCorrectAnalyticsPayerPayeesReturned()
    {
        var expectedPayerPayeeId1 = Guid.NewGuid().ToString();
        const string expectedPayerPayeeName1 = "name1";
        const int numberOfPayerPayee1Transactions = 6;
        const decimal payerPayee1TransactionsAmount = (decimal) 34.5;

        var expectedPayerPayeeId2 = Guid.NewGuid().ToString();
        const string expectedPayerPayeeName2 = "name2";
        const int numberOfPayerPayee2Transactions = 4;
        const decimal payerPayee2TransactionsAmount = (decimal) 34.5;

        string expectedPayerPayeeId3 = null;
        string expectedPayerPayeeName3 = null;
        const int numberOfPayerPayee3Transactions = 2;
        const decimal payerPayee3TransactionsAmount = (decimal) 34.5;

        var transactionList = new TransactionListBuilder()
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(numberOfPayerPayee1Transactions,
                expectedPayerPayeeId1, expectedPayerPayeeName1, payerPayee1TransactionsAmount)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(numberOfPayerPayee2Transactions,
                expectedPayerPayeeId2, expectedPayerPayeeName2, payerPayee2TransactionsAmount)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(numberOfPayerPayee3Transactions,
                expectedPayerPayeeId3, expectedPayerPayeeName3, payerPayee3TransactionsAmount)
            .Build();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["type"] = "expense";
        query["start"] = new DateTime(2021, 4, 1).ToString("yyyy-MM-dd");
        query["end"] = new DateTime(2099, 4, 1).ToString("yyyy-MM-dd");
        var queryString = query.ToString();

        var response = await _httpClient.GetAsync($"/api/analytics/payerpayees?{queryString}");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var actualAnalyticsPayerPayees = JsonSerializer.Deserialize<List<AnalyticsPayerPayee>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        var expectedAnalyticsPayerPayees = new List<AnalyticsPayerPayee>
        {
            new()
            {
                PayerPayeeId = expectedPayerPayeeId1,
                PayerPayeeName = expectedPayerPayeeName1,
                TotalAmount = numberOfPayerPayee1Transactions * payerPayee1TransactionsAmount
            },
            new()
            {
                PayerPayeeId = expectedPayerPayeeId2,
                PayerPayeeName = expectedPayerPayeeName2,
                TotalAmount = numberOfPayerPayee2Transactions * payerPayee2TransactionsAmount
            },
            new()
            {
                PayerPayeeId = "",
                PayerPayeeName = "Unspecified",
                TotalAmount = numberOfPayerPayee3Transactions * payerPayee3TransactionsAmount
            },
        };
        Assert.Equal(expectedAnalyticsPayerPayees, actualAnalyticsPayerPayees);
    }
}