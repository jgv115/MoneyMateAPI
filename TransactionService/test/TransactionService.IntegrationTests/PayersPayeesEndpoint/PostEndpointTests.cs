using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.Dtos;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace TransactionService.IntegrationTests.PayersPayeesEndpoint
{
    [Collection("IntegrationTests")]
    public class PostEndpointTests : IClassFixture<MoneyMateApiWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient _httpClient;
        private readonly DynamoDbHelper _dynamoDbHelper;
        private const string UserId = "auth0|moneymatetest#PayersPayees";

        public PostEndpointTests(MoneyMateApiWebApplicationFactory factory)
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

        [Theory]
        [InlineData("test payer123", "test external id 123")]
        [InlineData("test payer123", null)]
        public async Task
            GivenExistingPayersInDatabase_WhenPostPayerEndpointCalled_ThenDuplicatePayerShouldNotBePersisted(
                string expectedPayerName, string expectedExternalId)
        {
            var payer = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = $"payer#1234567",
                PayerPayeeName = expectedPayerName,
                ExternalId = expectedExternalId
            };
            var createPayerDto = new CreatePayerPayeeDto
            {
                Name = expectedPayerName,
                ExternalId = expectedExternalId
            };

            await _dynamoDbHelper.WriteIntoTable(new List<PayerPayee>
            {
                payer,
                new()
                {
                    ExternalId = "gkrdfhgjkdf",
                    PayerPayeeName = "name",
                    UserId = UserId,
                    PayerPayeeId = $"payer#12345676532",
                }
            });
            
            var httpContent =
                new StringContent(JsonSerializer.Serialize(createPayerDto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/payerspayees/payers", httpContent);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        
        [Theory]
        [InlineData("test payee123", "test external id 123")]
        [InlineData("test payee123", null)]
        public async Task
            GivenExistingPayeesInDatabase_WhenPostPayerEndpointCalled_ThenDuplicatePayeeShouldNotBePersisted(
                string expectedPayeeName, string expectedExternalId)
        {
            var payee = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = $"payee#1234567",
                PayerPayeeName = expectedPayeeName,
                ExternalId = expectedExternalId
            };
            var createPayerDto = new CreatePayerPayeeDto
            {
                Name = expectedPayeeName,
                ExternalId = expectedExternalId
            };

            await _dynamoDbHelper.WriteIntoTable(new List<PayerPayee>
            {
                payee,
                new()
                {
                    ExternalId = "gkrdfhgjkdf",
                    PayerPayeeName = "name",
                    UserId = UserId,
                    PayerPayeeId = $"payee#12345676532",
                }
            });
            
            var httpContent =
                new StringContent(JsonSerializer.Serialize(createPayerDto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/payerspayees/payees", httpContent);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
    }
}