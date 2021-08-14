using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.Models;
using Xunit;

namespace TransactionService.IntegrationTests.PayersPayeesEndpoint
{
    [Collection("IntegrationTests")]
    public class PayersPayeesEndpointTests : IClassFixture<WebApplicationFactory<Startup>>, IAsyncLifetime
    {
        private readonly HttpClient _httpClient;
        private readonly DynamoDbHelper _dynamoDbHelper = new();
        private const string UserId = "auth0|moneymatetest#PayersPayees";

        public PayersPayeesEndpointTests(WebApplicationFactory<Startup> factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.GetAccessToken();
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
        public async Task GivenValidRequest_WhenGetPayersEndpointCalled_ThenAllCorrectPayersReturned()
        {
            var payer1 = new PayerPayee
            {
                UserId = UserId,
                Name = "payer#payer1",
                GooglePlaceId = Guid.NewGuid().ToString()
            };
            var expectedPayer1 = new PayerPayee
            {
                UserId = UserId,
                Name = "payer1",
                GooglePlaceId = payer1.GooglePlaceId
            };
            var payer2 = new PayerPayee
            {
                UserId = UserId,
                Name = "payer#payer2",
                GooglePlaceId = Guid.NewGuid().ToString()
            };
            var expectedPayer2 = new PayerPayee
            {
                UserId = UserId,
                Name = "payer2",
                GooglePlaceId = payer2.GooglePlaceId
            };

            var initialData = new List<PayerPayee>
            {
                payer1,
                payer2,
                new()
                {
                    UserId = UserId,
                    Name = "payee#payee1",
                    GooglePlaceId = Guid.NewGuid().ToString()
                }
            };

            await _dynamoDbHelper.WriteIntoTable(initialData);

            var response = await _httpClient.GetAsync($"api/payerspayees/payers");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedPayers = JsonSerializer.Deserialize<List<PayerPayee>>(returnedString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal(new List<PayerPayee>
            {
                expectedPayer1, expectedPayer2
            }, returnedPayers);
        }

        [Fact]
        public async Task GivenValidRequest_WhenGetPayeesEndpointCalled_ThenCorrectPayeesReturned()
        {
            var payee1 = new PayerPayee
            {
                UserId = UserId,
                Name = "payee#payee1",
                GooglePlaceId = Guid.NewGuid().ToString()
            };
            var expectedPayee1 = new PayerPayee
            {
                UserId = UserId,
                Name = "payee1",
                GooglePlaceId = payee1.GooglePlaceId
            };
            var payee2 = new PayerPayee
            {
                UserId = UserId,
                Name = "payee#payee2",
                GooglePlaceId = Guid.NewGuid().ToString()
            };
            var expectedPayee2 = new PayerPayee
            {
                UserId = UserId,
                Name = "payee2",
                GooglePlaceId = payee2.GooglePlaceId
            };

            var initialData = new List<PayerPayee>
            {
                new()
                {
                    UserId = UserId,
                    Name = "payer#payer1",
                    GooglePlaceId = Guid.NewGuid().ToString()
                },
                new()
                {
                    UserId = UserId,
                    Name = "payer#payer2",
                    GooglePlaceId = Guid.NewGuid().ToString()
                },
                payee1,
                payee2
            };

            await _dynamoDbHelper.WriteIntoTable(initialData);

            var response = await _httpClient.GetAsync($"api/payerspayees/payees");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedPayees = JsonSerializer.Deserialize<List<PayerPayee>>(returnedString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal(new List<PayerPayee> {expectedPayee1, expectedPayee2}, returnedPayees);
        }
    }
}