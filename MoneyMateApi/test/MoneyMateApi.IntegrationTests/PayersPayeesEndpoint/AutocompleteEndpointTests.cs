using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MoneyMateApi.Controllers.PayersPayees.ViewModels;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.IntegrationTests.Extensions;
using MoneyMateApi.IntegrationTests.Helpers;
using MoneyMateApi.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace MoneyMateApi.IntegrationTests.PayersPayeesEndpoint;

[Collection("IntegrationTests")]
public class AutocompleteEndpointTests : IAsyncLifetime
{
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly HttpClient _httpClient;
    private const string ExpectedAddress = "1 Hello Street Vic Australia 3123";

    public AutocompleteEndpointTests(MoneyMateApiWebApplicationFactory factory)
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

    [Theory]
    [InlineData("test")]
    [InlineData("te")]
    [InlineData("t")]
    [InlineData("T")]
    public async Task
        GivenValidRequestWithSingleSearchWord_WhenGetAutocompletePayeesEndpointCalled_ThenCorrectPayeesReturned(
            string searchQuery)
    {
        var payee1 = new PayerPayee()
        {
            PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
            PayerPayeeName = "payee1",
            ExternalId = Guid.NewGuid().ToString()
        };
        var payee2 = new PayerPayee
        {
            PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
            PayerPayeeName = "Test2",
            ExternalId = Guid.NewGuid().ToString()
        };
        var expectedPayee2 = new PayerPayeeViewModel
        {
            PayerPayeeId = Guid.Parse("9540cf4a-f21b-4cac-9e8b-168d12dcecfc"),
            PayerPayeeName = "Test2",
            ExternalId = payee2.ExternalId,
            Address = ExpectedAddress
        };

        var initialData = new List<PayerPayee>
        {
            payee1,
            payee2
        };

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayeesIntoDb(initialData);
        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayersIntoDb(new List<PayerPayee>
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

        var response = await _httpClient.GetAsync($"/api/payerspayees/payees/autocomplete?name={searchQuery}");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedPayees = JsonSerializer.Deserialize<List<PayerPayeeViewModel>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(new List<PayerPayeeViewModel> {expectedPayee2}, returnedPayees);
    }

    [Theory]
    [InlineData("multiword")]
    [InlineData("multiword pa")]
    public async Task
        GivenValidRequestWithMultipleSearchWords_WhenGetAutocompletePayeesEndpointCalled_ThenCorrectPayeesReturned(
            string searchQuery)
    {
        var payee1 = new PayerPayee
        {
            PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
            PayerPayeeName = "payee1",
            ExternalId = Guid.NewGuid().ToString()
        };
        var payee2 = new PayerPayee
        {
            PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
            PayerPayeeName = "Multiword Payee",
            ExternalId = Guid.NewGuid().ToString()
        };
        var expectedPayee2 = new PayerPayeeViewModel
        {
            PayerPayeeId = Guid.Parse("9540cf4a-f21b-4cac-9e8b-168d12dcecfc"),
            PayerPayeeName = "Multiword Payee",
            ExternalId = payee2.ExternalId,
            Address = ExpectedAddress
        };

        var initialData = new List<PayerPayee>
        {
            payee1,
            payee2
        };

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayeesIntoDb(initialData);
        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayersIntoDb(new List<PayerPayee>
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

        var response = await _httpClient.GetAsync($"/api/payerspayees/payees/autocomplete?name={searchQuery}");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedPayees = JsonSerializer.Deserialize<List<PayerPayeeViewModel>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(new List<PayerPayeeViewModel> {expectedPayee2}, returnedPayees);
    }

    [Fact]
    public async Task
        GivenValidRequestWithSingleSearchWord_WhenGetAutocompletePayersEndpointCalled_ThenCorrectPayersReturned()
    {
        var payer1 = new PayerPayee()
        {
            PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
            PayerPayeeName = "payer1",
            ExternalId = Guid.NewGuid().ToString()
        };
        var payer2 = new PayerPayee
        {
            PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
            PayerPayeeName = "test2",
            ExternalId = Guid.NewGuid().ToString()
        };
        var expectedPayer2 = new PayerPayeeViewModel
        {
            PayerPayeeId = Guid.Parse("9540cf4a-f21b-4cac-9e8b-168d12dcecfc"),
            PayerPayeeName = "test2",
            ExternalId = payer2.ExternalId,
            Address = ExpectedAddress
        };

        var initialData = new List<PayerPayee>
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
            payer1,
            payer2
        };

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayersIntoDb(initialData);


        var response = await _httpClient.GetAsync($"/api/payerspayees/payers/autocomplete?name=test");
        response.EnsureSuccessStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedPayees = JsonSerializer.Deserialize<List<PayerPayeeViewModel>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(new List<PayerPayeeViewModel> {expectedPayer2}, returnedPayees);
    }

    [Theory]
    [InlineData("multiword")]
    [InlineData("multiword pa")]
    [InlineData("Multiword Pa")]
    [InlineData("multiword Payer")]
    public async Task
        GivenValidRequestWithMultipleSearchWords_WhenGetAutocompletePayersEndpointCalled_ThenCorrectPayersReturned(
            string searchQuery)
    {
        var payee1 = new PayerPayee()
        {
            PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfb",
            PayerPayeeName = "payer1",
            ExternalId = Guid.NewGuid().ToString()
        };
        var payer2 = new PayerPayee
        {
            PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcecfc",
            PayerPayeeName = "Multiword Payer",
            ExternalId = Guid.NewGuid().ToString()
        };
        var expectedPayer2 = new PayerPayeeViewModel
        {
            PayerPayeeId = Guid.Parse("9540cf4a-f21b-4cac-9e8b-168d12dcecfc"),
            PayerPayeeName = "Multiword Payer",
            ExternalId = payer2.ExternalId,
            Address = ExpectedAddress
        };

        var payer3 = new PayerPayee()
        {
            PayerPayeeId = "9540cf4a-f21b-4cac-9e8b-168d12dcec12",
            PayerPayeeName = "Multiword Payer 123",
            ExternalId = Guid.NewGuid().ToString()
        };
        var expectedPayer3 = new PayerPayeeViewModel
        {
            PayerPayeeId = Guid.Parse("9540cf4a-f21b-4cac-9e8b-168d12dcec12"),
            PayerPayeeName = "Multiword Payer 123",
            ExternalId = payer3.ExternalId,
            Address = ExpectedAddress
        };

        var initialData = new List<PayerPayee>
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
            payee1,
            payer2,
            payer3
        };

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayersIntoDb(initialData);

        var response = await _httpClient.GetAsync($"/api/payerspayees/payers/autocomplete?name={searchQuery}");
        await response.AssertSuccessfulStatusCode();

        var returnedString = await response.Content.ReadAsStringAsync();
        var returnedPayees = JsonSerializer.Deserialize<List<PayerPayeeViewModel>>(returnedString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.Equal(new List<PayerPayeeViewModel> {expectedPayer3, expectedPayer2}, returnedPayees);
    }
}