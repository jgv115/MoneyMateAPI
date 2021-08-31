using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using TransactionService.Dtos;
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
                PayerPayeeId = "payer#9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
                PayerPayeeName = "payer1",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayer1 = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
                PayerPayeeName = "payer1",
                ExternalId = payer1.ExternalId
            };
            var payer2 = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = "payer#9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
                PayerPayeeName = "payer2",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayer2 = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
                PayerPayeeName = "payer2",
                ExternalId = payer2.ExternalId
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
                PayerPayeeId = "payee#9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
                PayerPayeeName = "payee1",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayee1 = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
                PayerPayeeName = "payee1",
                ExternalId = payee1.ExternalId
            };
            var payee2 = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = "payee#9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
                PayerPayeeName = "payee2",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayee2 = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
                PayerPayeeName = "payee2",
                ExternalId = payee2.ExternalId
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
            var returnedPayees = JsonSerializer.Deserialize<List<PayerPayee>>(returnedString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal(new List<PayerPayee> {expectedPayee1, expectedPayee2}, returnedPayees);
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
            var expectedPayer = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = payerPayeeId.ToString(),
                PayerPayeeName = payer.PayerPayeeName,
                ExternalId = payer.ExternalId
            };
            await _dynamoDbHelper.WriteIntoTable(new List<PayerPayee> {payer});

            var response = await _httpClient.GetAsync($"/api/payerspayees/payers/{payerPayeeId.ToString()}");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var actualPayer = JsonSerializer.Deserialize<PayerPayee>(returnedString, new JsonSerializerOptions
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
            var expectedPayee = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = payerPayeeId.ToString(),
                PayerPayeeName = payee.PayerPayeeName,
                ExternalId = payee.ExternalId
            };
            await _dynamoDbHelper.WriteIntoTable(new List<PayerPayee> {payee});

            var response = await _httpClient.GetAsync($"/api/payerspayees/payees/{payerPayeeId.ToString()}");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var actualPayer = JsonSerializer.Deserialize<PayerPayee>(returnedString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.Equal(expectedPayee, actualPayer);
        }

        [Fact]
        public async Task GivenValidRequest_WhenPostPayerEndpointCalled_ThenCorrectPayerPersisted()
        {
            const string expectedPayerName = "test payer123";
            const string expectedExternalId = "test external id 123";
            var cratePayerDto = new CreatePayerPayeeDto
            {
                Name = expectedPayerName,
                ExternalId = expectedExternalId
            };
            var httpContent =
                new StringContent(JsonSerializer.Serialize(cratePayerDto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/payerspayees/payers", httpContent);
            response.EnsureSuccessStatusCode();

            var scanOutput = await _dynamoDbHelper.ScanTable<PayerPayee>();

            Assert.Collection(scanOutput,
                payerPayee =>
                {
                    Assert.Equal(expectedExternalId, payerPayee.ExternalId);
                    Assert.Equal(UserId, payerPayee.UserId);
                    Assert.Equal(expectedPayerName, payerPayee.PayerPayeeName);

                    var payerPayeeId = Guid.Parse(payerPayee.PayerPayeeId.Split("payer#")[1]);
                    Assert.NotEqual(Guid.Empty, payerPayeeId);
                });
        }

        [Fact]
        public async Task GivenRequestWithEmptyExternalId_WhenPostPayerEndpointCalled_ThenCorrectPayerPersisted()
        {
            const string expectedPayerName = "test payer123";
            var cratePayerDto = new CreatePayerPayeeDto
            {
                Name = expectedPayerName,
            };
            var httpContent =
                new StringContent(JsonSerializer.Serialize(cratePayerDto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/payerspayees/payers", httpContent);
            response.EnsureSuccessStatusCode();

            var scanOutput = await _dynamoDbHelper.ScanTable<PayerPayee>();

            Assert.Collection(scanOutput,
                payerPayee =>
                {
                    Assert.Null(payerPayee.ExternalId);
                    Assert.Equal(UserId, payerPayee.UserId);
                    Assert.Equal(expectedPayerName, payerPayee.PayerPayeeName);

                    var payerPayeeId = Guid.Parse(payerPayee.PayerPayeeId.Split("payer#")[1]);
                    Assert.NotEqual(Guid.Empty, payerPayeeId);
                });
        }

        [Fact]
        public async Task GivenValidRequest_WhenPostPayeeEndpointCalled_ThenCorrectPayeePersisted()
        {
            const string expectedPayeeName = "test payee123";
            const string expectedExternalId = "test external id 123";
            var cratePayerDto = new CreatePayerPayeeDto
            {
                Name = expectedPayeeName,
                ExternalId = expectedExternalId
            };
            var httpContent =
                new StringContent(JsonSerializer.Serialize(cratePayerDto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/payerspayees/payees", httpContent);
            response.EnsureSuccessStatusCode();

            var scanOutput = await _dynamoDbHelper.ScanTable<PayerPayee>();

            Assert.Collection(scanOutput,
                payerPayee =>
                {
                    Assert.Equal(expectedExternalId, payerPayee.ExternalId);
                    Assert.Equal(UserId, payerPayee.UserId);
                    Assert.Equal(expectedPayeeName, payerPayee.PayerPayeeName);

                    var payerPayeeId = Guid.Parse(payerPayee.PayerPayeeId.Split("payee#")[1]);
                    Assert.NotEqual(Guid.Empty, payerPayeeId);
                });
        }
    }
}