using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.Dtos;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using TransactionService.ViewModels;
using Xunit;

namespace TransactionService.IntegrationTests.PayersPayeesEndpoint
{
    [Collection("IntegrationTests")]
    public class PayersPayeesEndpointTests : IClassFixture<MoneyMateApiWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient HttpClient;
        private readonly DynamoDbHelper DynamoDbHelper;
        private const string UserId = "auth0|moneymatetest#PayersPayees";

        public PayersPayeesEndpointTests(MoneyMateApiWebApplicationFactory factory)
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
                ExternalId = payer1.ExternalId
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

            await DynamoDbHelper.WriteIntoTable(initialData);

            var response = await HttpClient.GetAsync($"api/payerspayees/payers");
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
                ExternalId = payee1.ExternalId
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

            await DynamoDbHelper.WriteIntoTable(initialData);

            var response = await HttpClient.GetAsync($"api/payerspayees/payees");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedPayees = JsonSerializer.Deserialize<List<PayerPayeeViewModel>>(returnedString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            Assert.Equal(new List<PayerPayeeViewModel> { expectedPayee1, expectedPayee2 }, returnedPayees);
        }

        [Fact]
        public async Task GivenValidRequest_WhenGetAutocompletePayeesEndpointCalled_ThenCorrectPayeesReturned()
        {
            var payee1 = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = "payee#9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
                PayerPayeeName = "payee1",
                ExternalId = Guid.NewGuid().ToString()
            };
            var payee2 = new PayerPayee
            {
                UserId = UserId,
                PayerPayeeId = "payee#9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
                PayerPayeeName = "test2",
                ExternalId = Guid.NewGuid().ToString()
            };
            var expectedPayee2 = new PayerPayeeViewModel
            {
                PayerPayeeId = Guid.Parse("9540cf4a-f21b-4cac-9e8b-168d12dcecfc"),
                PayerPayeeName = "test2",
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

            await DynamoDbHelper.WriteIntoTable(initialData);

            var response = await HttpClient.GetAsync($"/api/payerspayees/payees/autocomplete?name=test");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedPayees = JsonSerializer.Deserialize<List<PayerPayeeViewModel>>(returnedString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal(new List<PayerPayeeViewModel> { expectedPayee2 }, returnedPayees);
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
                ExternalId = payer.ExternalId
            };
            await DynamoDbHelper.WriteIntoTable(new List<PayerPayee> { payer });

            var response = await HttpClient.GetAsync($"/api/payerspayees/payers/{payerPayeeId.ToString()}");
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
                ExternalId = payee.ExternalId
            };
            await DynamoDbHelper.WriteIntoTable(new List<PayerPayee> { payee });

            var response = await HttpClient.GetAsync($"/api/payerspayees/payees/{payerPayeeId.ToString()}");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var actualPayer = JsonSerializer.Deserialize<PayerPayeeViewModel>(returnedString, new JsonSerializerOptions
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

            var response = await HttpClient.PostAsync("api/payerspayees/payers", httpContent);
            response.EnsureSuccessStatusCode();

            var scanOutput = await DynamoDbHelper.ScanTable<PayerPayee>();

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

            var response = await HttpClient.PostAsync("api/payerspayees/payers", httpContent);
            response.EnsureSuccessStatusCode();

            var scanOutput = await DynamoDbHelper.ScanTable<PayerPayee>();

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

            var response = await HttpClient.PostAsync("api/payerspayees/payees", httpContent);
            response.EnsureSuccessStatusCode();

            var scanOutput = await DynamoDbHelper.ScanTable<PayerPayee>();

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