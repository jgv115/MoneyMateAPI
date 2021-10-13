using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using TransactionService.Domain.Models;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using TransactionService.ViewModels;
using Xunit;

namespace TransactionService.IntegrationTests.AnalyticsEndpoint
{
    [Collection("IntegrationTests")]
    public class AnalyticsEndpointTests : IClassFixture<MoneyMateApiWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient HttpClient;
        private readonly DynamoDbHelper DynamoDbHelper;
        private const string UserId = "auth0|moneymatetest#Transaction";

        public AnalyticsEndpointTests(MoneyMateApiWebApplicationFactory factory)
        {
            HttpClient = factory.CreateDefaultClient();
            DynamoDbHelper = new DynamoDbHelper();
        }

        public async Task InitializeAsync()
        {
            await DynamoDbHelper.CreateTable();
        }

        public async Task DisposeAsync()
        {
            await DynamoDbHelper.DeleteTable();
        }

        [Fact]
        public async Task GivenInputParameters_WhenGetCategoryBreakdownEndpointInvoked_ThenCorrectCategoriesReturned()
        {
            const string expectedCategory1 = "category1";
            const int numberOfCategory1Transactions = 10;
            const decimal category1TransactionsAmount = (decimal)34.5;

            const string expectedCategory2 = "category2";
            const int numberOfCategory2Transactions = 5;
            const decimal category2TransactionsAmount = (decimal)34.5;

            const string expectedCategory3 = "category3";
            const int numberOfCategory3Transactions = 2;
            const decimal category3TransactionsAmount = (decimal)34.5;

            var transactionList = new TransactionListBuilder()
                .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory1Transactions, expectedCategory1,
                    category1TransactionsAmount)
                .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory2Transactions, expectedCategory2,
                    category2TransactionsAmount)
                .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory3Transactions, expectedCategory3,
                    category3TransactionsAmount)
                .Build();

            await DynamoDbHelper.WriteTransactionsIntoTable(transactionList);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["type"] = "expense";
            query["start"] = new DateTime(2021, 4, 1).ToString("yyyy-MM-dd");
            query["end"] = new DateTime(2099, 4, 1).ToString("yyyy-MM-dd");
            var queryString = query.ToString();

            var response = await HttpClient.GetAsync($"/api/analytics/categories?{queryString}");
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

        public class TransactionListBuilder
        {
            private readonly List<Transaction> _transactionList;

            public TransactionListBuilder()
            {
                _transactionList = new List<Transaction>();
            }

            public List<Transaction> Build()
            {
                return _transactionList;
            }

            public TransactionListBuilder WithNumberOfTransactionsOfCategoryAndAmount(int number, string category,
                decimal amount)
            {
                for (var i = 0; i < number; i++)
                {
                    _transactionList.Add(new Transaction
                    {
                        UserId = UserId,
                        TransactionId = Guid.NewGuid().ToString(),
                        TransactionTimestamp = DateTime.Now.ToString("O"),
                        TransactionType = "expense",
                        Amount = amount,
                        Category = category,
                        SubCategory = "subcategory-1",
                    });
                }

                return this;
            }

            public TransactionListBuilder WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(int number,
                string category, string subcategory,
                decimal amount)
            {
                for (var i = 0; i < number; i++)
                {
                    _transactionList.Add(new Transaction
                    {
                        UserId = UserId,
                        TransactionId = Guid.NewGuid().ToString(),
                        TransactionTimestamp = DateTime.Now.ToString("O"),
                        TransactionType = "expense",
                        Amount = amount,
                        Category = category,
                        SubCategory = subcategory,
                    });
                }

                return this;
            }
        }
    }
}