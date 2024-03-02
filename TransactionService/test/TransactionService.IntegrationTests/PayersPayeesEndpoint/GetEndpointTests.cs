using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TransactionService.Constants;
using TransactionService.Controllers.PayersPayees.ViewModels;
using TransactionService.Domain.Models;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using Xunit;
using Xunit.Abstractions;

namespace TransactionService.IntegrationTests.PayersPayeesEndpoint;

[Collection("IntegrationTests")]
public class GetEndpointTests : IAsyncLifetime
{
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly HttpClient _httpClient;
    private const string ExpectedAddress = "1 Hello Street Vic Australia 3123";

    public GetEndpointTests(MoneyMateApiWebApplicationFactory factory,
        ITestOutputHelper testOutputHelper)
    {
        var factory2 = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>()
                {
                    ["CockroachDb:Enabled"] = "true"
                })));
        _httpClient = factory2.CreateDefaultClient();
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
    public async Task GivenValidRequest_WhenGetPayersEndpointCalled_ThenAllCorrectPayersReturned()
    {
        var payer1 = new PayerPayee
        {
            PayerPayeeId = "50f5b2d0-b8ce-41ee-a17b-d97d466063cb",
            PayerPayeeName = "payer1",
            ExternalId = Guid.NewGuid().ToString()
        };
        var expectedPayer1 = new PayerPayeeViewModel
        {
            PayerPayeeId = Guid.Parse(payer1.PayerPayeeId),
            PayerPayeeName = "payer1",
            ExternalId = payer1.ExternalId,
            Address = ExpectedAddress
        };
        var payer2 = new PayerPayee
        {
            PayerPayeeId = "cadb756c-b6e2-42be-aaaa-9a70db22f308",
            PayerPayeeName = "payer2",
            ExternalId = Guid.NewGuid().ToString()
        };
        var expectedPayer2 = new PayerPayeeViewModel
        {
            PayerPayeeId = Guid.Parse(payer2.PayerPayeeId),
            PayerPayeeName = "payer2",
            ExternalId = payer2.ExternalId,
            Address = ExpectedAddress
        };

        await _cockroachDbIntegrationTestHelper.WritePayersIntoDb(new List<PayerPayee>
        {
            payer1,
            payer2,
        });
        await _cockroachDbIntegrationTestHelper.WritePayeesIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = "a540cf4a-f21b-4cac-9e8b-168d12dcecff",
                PayerPayeeName = "payee1",
                ExternalId = Guid.NewGuid().ToString()
            }
        });

        var response = await _httpClient.GetAsync($"api/payerspayees/payers");
        await response.AssertSuccessfulStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedPayers = JsonSerializer.Deserialize<List<PayerPayeeViewModel>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        //
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
            PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
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
            PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
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

        await _cockroachDbIntegrationTestHelper.WritePayeesIntoDb(new List<PayerPayee> {payee1, payee2});
        await _cockroachDbIntegrationTestHelper.WritePayersIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfd",
                PayerPayeeName = "payer1",
                ExternalId = Guid.NewGuid().ToString()
            },
            new()
            {
                PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfe",
                PayerPayeeName = "payer2",
                ExternalId = Guid.NewGuid().ToString()
            },
        });

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
        var payer = new PayerPayee()
        {
            PayerPayeeId = payerPayeeId.ToString(),
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
        await _cockroachDbIntegrationTestHelper.WritePayersIntoDb(new List<PayerPayee> {payer});

        var response = await _httpClient.GetAsync($"/api/payerspayees/payers/{payerPayeeId.ToString()}");
        await response.AssertSuccessfulStatusCode();

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
        var payee = new PayerPayee()
        {
            PayerPayeeId = payerPayeeId.ToString(),
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
        await _cockroachDbIntegrationTestHelper.WritePayeesIntoDb(new List<PayerPayee> {payee});

        var response = await _httpClient.GetAsync($"/api/payerspayees/payees/{payerPayeeId.ToString()}");
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Fail(
                $"Received a non successful status code: {(int) response.StatusCode} with body: {responseBody}");
        }

        var returnedString = await response.Content.ReadAsStringAsync();
        var actualPayer = JsonSerializer.Deserialize<PayerPayeeViewModel>(returnedString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.Equal(expectedPayee, actualPayer);
    }
}