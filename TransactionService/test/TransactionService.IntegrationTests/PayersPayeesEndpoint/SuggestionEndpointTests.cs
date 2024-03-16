using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TransactionService.Constants;
using TransactionService.Controllers.PayersPayees.ViewModels;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using TransactionService.Tests.Common;
using Xunit;

namespace TransactionService.IntegrationTests.PayersPayeesEndpoint;

[Collection("IntegrationTests")]
public class SuggestionEndpointTests : IAsyncLifetime
{
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly HttpClient _httpClient;

    public SuggestionEndpointTests(MoneyMateApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
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
    public async Task GivenRequest_WhenGetSuggestedPayersEndpointCalled_ThenCorrectPayersReturned()
    {
        var payer1 = Guid.NewGuid();
        var payer2 = Guid.NewGuid();
        var transactionListBuilder = new TransactionListBuilder();
        var transactions = transactionListBuilder
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(6, payer1.ToString(), "name", 20,
                TransactionType.Income)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(5, payer2.ToString(), "name2", 20,
                TransactionType.Income)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(7, Guid.NewGuid().ToString(), "name2", 20)
            .Build();
        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactions);

        var response = await _httpClient.GetAsync($"/api/payerspayees/payers/suggestions");
        await response.AssertSuccessfulStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedPayees = JsonSerializer.Deserialize<List<PayerPayeeViewModel>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(new List<PayerPayeeViewModel>
        {
            new()
            {
                PayerPayeeId = payer1,
                PayerPayeeName = "name"
            },
            new()
            {
                PayerPayeeId = payer2,
                PayerPayeeName = "name2"
            }
        }, returnedPayees);
    }

    [Fact]
    public async Task GivenRequest_WhenGetSuggestedPayeesEndpointCalled_ThenCorrectPayeesReturned()
    {
        var payee1 = Guid.NewGuid();
        var payee2 = Guid.NewGuid();
        var transactionListBuilder = new TransactionListBuilder();
        var transactions = transactionListBuilder
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(6, payee1.ToString(), "name", 20)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(5, payee2.ToString(), "name2", 20)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(7, Guid.NewGuid().ToString(), "name2", 20,
                TransactionType.Income)
            .Build();
        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactions);

        var response = await _httpClient.GetAsync($"/api/payerspayees/payees/suggestions");
        await response.AssertSuccessfulStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedPayees = JsonSerializer.Deserialize<List<PayerPayeeViewModel>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(new List<PayerPayeeViewModel>
        {
            new()
            {
                PayerPayeeId = payee1,
                PayerPayeeName = "name"
            },
            new()
            {
                PayerPayeeId = payee2,
                PayerPayeeName = "name2"
            }
        }, returnedPayees);
    }
}