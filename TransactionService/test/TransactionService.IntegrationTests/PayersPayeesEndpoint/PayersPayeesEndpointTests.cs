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
                Name = "payer#payer1",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayer1 = new PayerPayee
            {
                UserId = UserId,
                Name = "payer1",
                ExternalId = payer1.ExternalId
            };
            var payer2 = new PayerPayee
            {
                UserId = UserId,
                Name = "payer#payer2",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayer2 = new PayerPayee
            {
                UserId = UserId,
                Name = "payer2",
                ExternalId = payer2.ExternalId
            };

            var initialData = new List<PayerPayee>
            {
                payer1,
                payer2,
                new()
                {
                    UserId = UserId,
                    Name = "payee#payee1",
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
                Name = "payee#payee1",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayee1 = new PayerPayee
            {
                UserId = UserId,
                Name = "payee1",
                ExternalId = payee1.ExternalId
            };
            var payee2 = new PayerPayee
            {
                UserId = UserId,
                Name = "payee#payee2",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayee2 = new PayerPayee
            {
                UserId = UserId,
                Name = "payee2",
                ExternalId = payee2.ExternalId
            };

            var initialData = new List<PayerPayee>
            {
                new()
                {
                    UserId = UserId,
                    Name = "payer#payer1",
                    ExternalId = Guid.NewGuid().ToString()
                },
                new()
                {
                    UserId = UserId,
                    Name = "payer#payer2",
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

            Assert.Equal(new List<PayerPayee>
            {
                new()
                {
                    Name = $"payer#{expectedPayerName}",
                    ExternalId = expectedExternalId,
                    UserId = UserId
                }
            }, scanOutput);
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

            Assert.Equal(new List<PayerPayee>
            {
                new()
                {
                    Name = $"payer#{expectedPayerName}",
                    UserId = UserId
                }
            }, scanOutput);
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

            Assert.Equal(new List<PayerPayee>
            {
                new()
                {
                    Name = $"payee#{expectedPayeeName}",
                    ExternalId = expectedExternalId,
                    UserId = UserId
                }
            }, scanOutput);
        }
    }
}