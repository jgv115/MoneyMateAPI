using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using TransactionService.Dtos;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.Models;
using Xunit;

namespace TransactionService.IntegrationTests.TransactionsEndpoint
{
    [Collection("IntegrationTests")]
    public class TransactionsEndpointTests : IClassFixture<WebApplicationFactory<Startup>>, IAsyncLifetime
    {
        private readonly HttpClient _httpClient;
        private readonly DynamoDbHelper _dynamoDbHelper;
        private const string UserId = "auth0|moneymatetest#Transaction";

        public TransactionsEndpointTests(WebApplicationFactory<Startup> factory)
        {
            // // Uncomment to run integration test class locally
            // factory = factory.WithWebHostBuilder(builder =>
            // {
            //     builder.ConfigureAppConfiguration((context, configurationBuilder) =>
            //     {
            //         Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "dev");
            //         configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            //         {
            //             {"Auth0:Authority", "https://moneymate-dev.au.auth0.com/"},
            //             {"Auth0:Audience", "https://api.dev.moneymate.benong.id.au"},
            //             {"DynamoDb:LocalMode", "true"},
            //             {"DynamoDb:ServiceUrl", "http://localhost:4566"}
            //         });
            //     });
            // });

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

        [Fact]
        public async Task GivenNoInputParameters_WhenGetTransactionsIsCalled_ThenAllTransactionsAreReturned()
        {
            var transaction1 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };

            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2
            };
            await _dynamoDbHelper.WriteTransactionsIntoTable(transactionList);

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
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };

            var transaction3 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
                TransactionTimestamp = new DateTime(2021, 3, 1).ToString("O"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };

            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2,
                transaction3
            };

            await _dynamoDbHelper.WriteTransactionsIntoTable(transactionList);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["start"] = new DateTime(2021, 4, 1).ToString("O");
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
            var transaction1 = new Transaction
            {
                UserId = UserId,
                TransactionTimestamp = new DateTime(2021, 3, 1).ToString("O"),
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = new DateTime(2021, 3, 2).ToString("O"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };
            var transaction3 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
                TransactionTimestamp = new DateTime(2021, 3, 2).ToString("O"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };

            var transaction4 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915d",
                TransactionTimestamp = new DateTime(2021, 3, 5).ToString("O"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };

            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2,
                transaction3,
                transaction4
            };

            await _dynamoDbHelper.WriteTransactionsIntoTable(transactionList);

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

            var expectedTransactionList = new List<Transaction>
            {
                transaction1, transaction2, transaction3
            };
            Assert.Equal(expectedTransactionList, returnedTransactionList);
        }

        [Fact]
        public async Task
            GivenValidTransactionRequest_WhenPostTransactionsIsCalled_ThenCorrectTransactionsArePersisted()
        {
            const decimal expectedAmount = 123M;
            const string expectedCategory = "Food";
            const string expectedSubCategory = "Dinner";
            var expectedTransactionTimestamp = new DateTime(2021, 4, 2).ToString("O");
            const string expectedTransactionType = "expense";
            var inputDto = new StoreTransactionDto
            {
                Amount = expectedAmount,
                Category = expectedCategory,
                SubCategory = expectedSubCategory,
                TransactionTimestamp = expectedTransactionTimestamp,
                TransactionType = expectedTransactionType
            };

            string inputDtoString = JsonSerializer.Serialize(inputDto);

            StringContent httpContent =
                new StringContent(inputDtoString, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/api/transactions", httpContent);
            response.EnsureSuccessStatusCode();

            var returnedTransactions = await _dynamoDbHelper.ScanTable();

            Assert.Single(returnedTransactions);
            Assert.Equal(expectedAmount, returnedTransactions[0].Amount);
            Assert.Equal(expectedCategory, returnedTransactions[0].Category);
            Assert.Equal(expectedSubCategory, returnedTransactions[0].SubCategory);
            Assert.Equal(expectedTransactionTimestamp, returnedTransactions[0].TransactionTimestamp);
            Assert.Equal(expectedTransactionType, returnedTransactions[0].TransactionType);
        }

        [Fact]
        public async Task GivenValidPutTransactionDto_WhenPutTransactionsIsCalled_ThenCorrecTransactionsArePersisted()
        {
            var expectedTransactionId = Guid.NewGuid().ToString();

            var transaction1 = new Transaction
            {
                UserId = UserId,
                TransactionId = expectedTransactionId,
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };

            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2
            };
            await _dynamoDbHelper.WriteTransactionsIntoTable(transactionList);

            const decimal expectedAmount = 123M;
            const string expectedCategory = "Food";
            const string expectedSubCategory = "Dinner";
            var expectedTransactionTimestamp = new DateTime(2021, 4, 2).ToString("O");
            const string expectedTransactionType = "expense";
            var inputDto = new PutTransactionDto
            {
                Amount = expectedAmount,
                Category = expectedCategory,
                SubCategory = expectedSubCategory,
                TransactionTimestamp = expectedTransactionTimestamp,
                TransactionType = expectedTransactionType
            };

            string inputDtoString = JsonSerializer.Serialize(inputDto);
            StringContent httpContent = new StringContent(inputDtoString, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/transactions/{expectedTransactionId}", httpContent);
            response.EnsureSuccessStatusCode();

            var returnedTransaction = await _dynamoDbHelper.QueryTable(UserId, expectedTransactionId);
            var expectedTransaction = new Transaction
            {
                UserId = UserId,
                TransactionId = expectedTransactionId,
                Amount = expectedAmount,
                Category = expectedCategory,
                SubCategory = expectedSubCategory,
                TransactionTimestamp = expectedTransactionTimestamp,
                TransactionType = expectedTransactionType
            };
            Assert.Equal(expectedTransaction, returnedTransaction);
        }
    }
}