using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TransactionService.Controllers.PayersPayees.ViewModels;
using TransactionService.Domain.Models;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace TransactionService.IntegrationTests.PayersPayeesEndpoint
{
    [Collection("IntegrationTests")]
    public class GetEndpointTests : IClassFixture<MoneyMateApiWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient _httpClient;
        private readonly DynamoDbHelper _dynamoDbHelper;
        private const string UserId = "auth0|moneymatetest#PayersPayees";
        private const string ExpectedAddress = "1 Hello Street Vic Australia 3123";

        public GetEndpointTests(MoneyMateApiWebApplicationFactory factory)
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

        [Fact]
        public async Task GivenValidRequest_WhenGetPayersEndpointCalled_ThenAllCorrectPayersReturned()
        {
            var payer1 = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = "payer#9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
                PayerPayeeName = "payer1",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayer1 = new PayerPayeeViewModel
            {
                PayerPayeeId = Guid.Parse("9540cf4a-f21b-4cac-9e8b-168d12dcecfb"),
                PayerPayeeName = "payer1",
                ExternalId = payer1.ExternalId,
                Address = ExpectedAddress
            };
            var payer2 = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = "payer#9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
                PayerPayeeName = "payer2",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayer2 = new PayerPayeeViewModel
            {
                PayerPayeeId = Guid.Parse("9540cf4a-f21b-4cac-9e8b-168d12dcecfc"),
                PayerPayeeName = "payer2",
                ExternalId = payer2.ExternalId,
                Address = ExpectedAddress
            };

            var initialData = new List<PayerPayee>
            {
                payer1,
                payer2,
                new()
                {
                    UserId = UserId,
                    PayerPayeeId = "payee#a540cf4a-f21b-4cac-9e8b-168d12dcecff",
                    PayerPayeeName = "payee1",
                    ExternalId = Guid.NewGuid().ToString()
                }
            };

            await _dynamoDbHelper.WriteIntoTable(initialData);

            var response = await _httpClient.GetAsync($"api/payerspayees/payers");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedPayers = JsonSerializer.Deserialize<List<PayerPayeeViewModel>>(returnedString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            Assert.Equal(new List<PayerPayeeViewModel>
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
                PayerPayeeId = "payee#9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
                PayerPayeeName = "payee1",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayee1 = new PayerPayeeViewModel
            {
                PayerPayeeId = Guid.Parse("9540cf4a-f21b-4cac-9e8b-168d12dcecfb"),
                PayerPayeeName = "payee1",
                ExternalId = payee1.ExternalId,
                Address = ExpectedAddress
            };
            var payee2 = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = "payee#9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
                PayerPayeeName = "payee2",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayee2 = new PayerPayeeViewModel
            {
                PayerPayeeId = Guid.Parse("9540cf4a-f21b-4cac-9e8b-168d12dcecfc"),
                PayerPayeeName = "payee2",
                ExternalId = payee2.ExternalId,
                Address = ExpectedAddress
            };

            var initialData = new List<PayerPayee>
            {
                new()
                {
                    UserId = UserId,
                    PayerPayeeId = "payer#9540cf4a-f21b-4cac-9e8b-168d12dcecfd",
                    PayerPayeeName = "payer1",
                    ExternalId = Guid.NewGuid().ToString()
                },
                new()
                {
                    UserId = UserId,
                    PayerPayeeId = "payer#9540cf4a-f21b-4cac-9e8b-168d12dcecfe",
                    PayerPayeeName = "payer2",
                    ExternalId = Guid.NewGuid().ToString()
                },
                payee1,
                payee2
            };

            await _dynamoDbHelper.WriteIntoTable(initialData);

            var response = await _httpClient.GetAsync($"api/payerspayees/payees");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedPayees = JsonSerializer.Deserialize<List<PayerPayeeViewModel>>(returnedString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            Assert.Equal(new List<PayerPayeeViewModel> {expectedPayee1, expectedPayee2}, returnedPayees);
        }

        [Fact]
        public async Task GivenValidRequest_WhenGetPayerEndpointCalled_ThenCorrectPayerReturned()
        {
            var payerPayeeId = Guid.NewGuid();
            var payer = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = $"payer#{payerPayeeId.ToString()}",
                PayerPayeeName = "payer1",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayer = new PayerPayeeViewModel
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = payer.PayerPayeeName,
                ExternalId = payer.ExternalId,
                Address = "1 Hello Street Vic Australia 3123"
            };
            await _dynamoDbHelper.WriteIntoTable(new List<PayerPayee> {payer});

            var response = await _httpClient.GetAsync($"/api/payerspayees/payers/{payerPayeeId.ToString()}");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var actualPayer = JsonSerializer.Deserialize<PayerPayeeViewModel>(returnedString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal(expectedPayer, actualPayer);
        }

        [Fact]
        public async Task GivenValidRequest_WhenGetPayeeEndpointCalled_ThenCorrectPayeeReturned()
        {
            var payerPayeeId = Guid.NewGuid();
            var payee = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = $"payee#{payerPayeeId.ToString()}",
                PayerPayeeName = "payee1",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayee = new PayerPayeeViewModel
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = payee.PayerPayeeName,
                ExternalId = payee.ExternalId,
                Address = "1 Hello Street Vic Australia 3123"
            };
            await _dynamoDbHelper.WriteIntoTable(new List<PayerPayee> {payee});

            var response = await _httpClient.GetAsync($"/api/payerspayees/payees/{payerPayeeId.ToString()}");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var actualPayer = JsonSerializer.Deserialize<PayerPayeeViewModel>(returnedString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal(expectedPayee, actualPayer);
        }
    }
}