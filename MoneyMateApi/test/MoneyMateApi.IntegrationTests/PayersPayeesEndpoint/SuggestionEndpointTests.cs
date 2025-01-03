using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.PayersPayees.ViewModels;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.IntegrationTests.Extensions;
using MoneyMateApi.IntegrationTests.Helpers;
using MoneyMateApi.IntegrationTests.WebApplicationFactories;
using MoneyMateApi.Tests.Common;
using Xunit;

namespace MoneyMateApi.IntegrationTests.PayersPayeesEndpoint;

[Collection("IntegrationTests")]
public class SuggestionEndpointTests : IAsyncLifetime
{
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly HttpClient _httpClient;

    // Hard coded from the Google API mock
    // private const string PayerPayeeExternalId = "externalId-123";
    private const string PayerPayeeAddress = "1 Hello Street Vic Australia 3123";

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
        var payee1 = Guid.NewGuid();

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayersIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = payer1.ToString(),
                PayerPayeeName = "name",
                ExternalId = "externalId-123"
            },
            new()
            {
                PayerPayeeId = payer2.ToString(),
                PayerPayeeName = "name2",
                ExternalId = "externalId-345"
            },
        });

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayeesIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = payee1.ToString(),
                PayerPayeeName = "name2"
            }
        });

        var transactionListBuilder = new TransactionListBuilder();
        var transactions = transactionListBuilder
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(6, payer1.ToString(), "name", 20,
                TransactionType.Income)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(5, payer2.ToString(), "name2", 20,
                TransactionType.Income)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(7, payee1.ToString(), "name2", 20)
            .BuildDomainModels();
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
                PayerPayeeName = "name",
                ExternalId = "externalId-123",
                Address = PayerPayeeAddress
            },
            new()
            {
                PayerPayeeId = payer2,
                PayerPayeeName = "name2",
                ExternalId = "externalId-345",
                Address = PayerPayeeAddress
            }
        }, returnedPayees);
    }

    [Fact]
    public async Task
        GivenRequestWithEnrichedDataDisabled_WhenGetSuggestedPayersEndpointCalled_ThenCorrectPayersReturned()
    {
        var payer1 = Guid.NewGuid();
        var payer2 = Guid.NewGuid();
        var payee1 = Guid.NewGuid();

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayersIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = payer1.ToString(),
                PayerPayeeName = "name",
                ExternalId = "externalId-123"
            },
            new()
            {
                PayerPayeeId = payer2.ToString(),
                PayerPayeeName = "name2",
                ExternalId = "externalId-345"
            },
        });

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayeesIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = payee1.ToString(),
                PayerPayeeName = "name2"
            }
        });

        var transactionListBuilder = new TransactionListBuilder();
        var transactions = transactionListBuilder
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(6, payer1.ToString(), "name", 20,
                TransactionType.Income)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(5, payer2.ToString(), "name2", 20,
                TransactionType.Income)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(7, payee1.ToString(), "name2", 20)
            .BuildDomainModels();
        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactions);

        var response = await _httpClient.GetAsync($"/api/payerspayees/payers/suggestions?includeEnrichedData=false");
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
                PayerPayeeName = "name",
            },
            new()
            {
                PayerPayeeId = payer2,
                PayerPayeeName = "name2",
            }
        }, returnedPayees);
    }


    [Fact]
    public async Task GivenRequest_WhenGetSuggestedPayeesEndpointCalled_ThenCorrectPayeesReturned()
    {
        var payee1 = Guid.NewGuid();
        var payee2 = Guid.NewGuid();
        var payer1 = Guid.NewGuid();

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayeesIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = payee1.ToString(),
                PayerPayeeName = "name",
                ExternalId = "externalId-123"
            },
            new()
            {
                PayerPayeeId = payee2.ToString(),
                PayerPayeeName = "name2",
                ExternalId = "externalId-345"
            },
        });

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayersIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = payer1.ToString(),
                PayerPayeeName = "name2"
            }
        });

        var transactionListBuilder = new TransactionListBuilder();
        var transactions = transactionListBuilder
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(6, payee1.ToString(), "name", 20)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(5, payee2.ToString(), "name2", 20)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(7, payer1.ToString(), "name2", 20,
                TransactionType.Income)
            .BuildDomainModels();
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
                PayerPayeeName = "name",
                ExternalId = "externalId-123",
                Address = PayerPayeeAddress
            },
            new()
            {
                PayerPayeeId = payee2,
                PayerPayeeName = "name2",
                ExternalId = "externalId-345",
                Address = PayerPayeeAddress
            }
        }, returnedPayees);
    }

    [Fact]
    public async Task
        GivenRequestWithEnrichedDataDisabled_WhenGetSuggestedPayeesEndpointCalled_ThenCorrectPayersReturned()
    {
        var payee1 = Guid.NewGuid();
        var payee2 = Guid.NewGuid();
        var payer1 = Guid.NewGuid();

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayeesIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = payee1.ToString(),
                PayerPayeeName = "name",
                ExternalId = "externalId-123"
            },
            new()
            {
                PayerPayeeId = payee2.ToString(),
                PayerPayeeName = "name2",
                ExternalId = "externalId-345"
            },
        });

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayersIntoDb(new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = payer1.ToString(),
                PayerPayeeName = "name2"
            }
        });

        var transactionListBuilder = new TransactionListBuilder();
        var transactions = transactionListBuilder
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(6, payee1.ToString(), "name", 20,
                TransactionType.Expense)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(5, payee2.ToString(), "name2", 20,
                TransactionType.Expense)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(7, payer1.ToString(), "name2", 20,
                TransactionType.Income)
            .BuildDomainModels();
        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactions);

        var response = await _httpClient.GetAsync($"/api/payerspayees/payees/suggestions?includeEnrichedData=false");
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
                PayerPayeeName = "name",
            },
            new()
            {
                PayerPayeeId = payee2,
                PayerPayeeName = "name2",
            }
        }, returnedPayees);
    }
}