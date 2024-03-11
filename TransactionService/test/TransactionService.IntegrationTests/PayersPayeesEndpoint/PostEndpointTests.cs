using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TransactionService.Constants;
using TransactionService.Controllers.PayersPayees.Dtos;
using TransactionService.Controllers.PayersPayees.ViewModels;
using TransactionService.Domain.Models;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace TransactionService.IntegrationTests.PayersPayeesEndpoint;

[Collection("IntegrationTests")]
public class PostEndpointTests : IAsyncLifetime
{
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly HttpClient _httpClient;
    private const string ExpectedAddress = "1 Hello Street Vic Australia 3123";

    public PostEndpointTests(MoneyMateApiWebApplicationFactory factory)
    {
        _httpClient = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>()
                {
                    ["CockroachDb:Enabled"] = "true"
                }))).CreateClient();
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
        GivenRequestWithEmptyExternalId_WhenPostPayerEndpointCalled_ThenCorrectPayerPersistedAndReturned()
    {
        const string expectedPayerName = "test payer123";
        var cratePayerDto = new CreatePayerPayeeDto
        {
            Name = expectedPayerName,
        };
        var httpContent =
            new StringContent(JsonSerializer.Serialize(cratePayerDto), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/payerspayees/payers", httpContent);
        await response.AssertSuccessfulStatusCode();

        var scanOutput = await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.RetrieveAllPayersPayees("payer");
        Assert.Collection(scanOutput,
            payerPayee =>
            {
                Assert.Empty(payerPayee.ExternalId);
                Assert.Equal(expectedPayerName, payerPayee.PayerPayeeName);

                var payerPayeeId = Guid.Parse(payerPayee.PayerPayeeId);
                Assert.NotEqual(Guid.Empty, payerPayeeId);
            });

        var savedPayer = scanOutput.First();
        var returnedPayer = await response.Content.ReadFromJsonAsync<PayerPayeeViewModel>();
        Assert.Equal(new PayerPayeeViewModel
        {
            PayerPayeeId = Guid.Parse(savedPayer.PayerPayeeId),
            PayerPayeeName = savedPayer.PayerPayeeName
        }, returnedPayer);
    }


    [Fact]
    public async Task GivenValidRequest_WhenPostPayeeEndpointCalled_ThenCorrectPayeePersistedAndReturned()
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
        await response.AssertSuccessfulStatusCode();

        var scanOutput = await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.RetrieveAllPayersPayees("payee");

        Assert.Collection(scanOutput,
            payerPayee =>
            {
                Assert.Equal(expectedExternalId, payerPayee.ExternalId);
                Assert.Equal(expectedPayeeName, payerPayee.PayerPayeeName);

                var payerPayeeId = Guid.Parse(payerPayee.PayerPayeeId);
                Assert.NotEqual(Guid.Empty, payerPayeeId);
            });

        var savedPayee = scanOutput.First();
        var returnedPayer = await response.Content.ReadFromJsonAsync<PayerPayeeViewModel>();
        Assert.Equal(new PayerPayeeViewModel
        {
            PayerPayeeId = Guid.Parse(savedPayee.PayerPayeeId),
            PayerPayeeName = savedPayee.PayerPayeeName,
            ExternalId = expectedExternalId,
            Address = ExpectedAddress
        }, returnedPayer);
    }

    [Theory]
    [InlineData("test payee123", "test external id 123")]
    [InlineData("test payee123", "")]
    public async Task
        GivenExistingPayeesInDatabase_WhenPostPayeeEndpointCalled_ThenDuplicatePayeeShouldNotBePersisted(
            string expectedPayeeName, string expectedExternalId)
    {
        var payee = new PayerPayee()
        {
            PayerPayeeId = $"28cde610-d83a-42c0-b474-c702cc61d0bd",
            PayerPayeeName = expectedPayeeName,
            ExternalId = expectedExternalId
        };
        var createPayerDto = new CreatePayerPayeeDto
        {
            Name = expectedPayeeName,
            ExternalId = expectedExternalId
        };

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayeesIntoDb(new List<PayerPayee>
        {
            payee,
            new()
            {
                ExternalId = "gkrdfhgjkdf",
                PayerPayeeName = "name",
                PayerPayeeId = $"38cde610-d83a-42c0-b474-c702cc61d0bc",
            }
        });

        var httpContent =
            new StringContent(JsonSerializer.Serialize(createPayerDto), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/payerspayees/payees", httpContent);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}