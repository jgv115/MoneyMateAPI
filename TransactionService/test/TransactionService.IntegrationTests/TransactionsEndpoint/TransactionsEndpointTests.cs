using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.Models;
using Xunit;

namespace TransactionService.IntegrationTests.TransactionsEndpoint
{
    public class TransactionsEndpointTests : IClassFixture<WebApplicationFactory<Startup>>, IAsyncLifetime
    {
        private readonly HttpClient _httpClient;
        private readonly DynamoDbHelper _dynamoDbHelper;

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
                UserId = "auth0|moneymatetest#Transaction",
                Date = DateTime.Now.ToString("o"),
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };
            var transaction2 = new Transaction
            {
                UserId = "auth0|moneymatetest#Transaction",
                Date = DateTime.Now.ToString("o"),
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
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
                UserId = "auth0|moneymatetest#Transaction",
                Date = DateTime.Now.ToString("o"),
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };
            var transaction2 = new Transaction
            {
                UserId = "auth0|moneymatetest#Transaction",
                Date = DateTime.Now.ToString("o"),
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };

            var transaction3 = new Transaction
            {
                UserId = "auth0|moneymatetest#Transaction",
                Date = new DateTime(2021, 3, 1).ToString("O"),
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
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
                UserId = "auth0|moneymatetest#Transaction",
                Date = new DateTime(2021, 3, 1).ToString("O"),
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };
            var transaction2 = new Transaction
            {
                UserId = "auth0|moneymatetest#Transaction",
                Date = new DateTime(2021, 3, 2).ToString("O"),
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                SubCategory = "Meat"
            };

            var transaction3 = new Transaction
            {
                UserId = "auth0|moneymatetest#Transaction",
                Date = new DateTime(2021, 3, 5).ToString("O"),
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
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

            var expectedTransactionList = new List<Transaction>
            {
                transaction1, transaction2
            };
            Assert.Equal(expectedTransactionList, returnedTransactionList);
        }
    }
}