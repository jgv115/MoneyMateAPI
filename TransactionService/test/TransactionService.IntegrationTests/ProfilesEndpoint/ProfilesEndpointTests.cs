using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace TransactionService.IntegrationTests.ProfilesEndpoint;

[Collection("IntegrationTests")]
public class ProfilesEndpointTests : IClassFixture<MoneyMateApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly Guid _testUserId;

    public ProfilesEndpointTests(MoneyMateApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
        _cockroachDbIntegrationTestHelper = factory.CockroachDbIntegrationTestHelper;
        _testUserId = factory.TestUserId;
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
    public async Task GivenGetProfilesCalled_ThenDefaultProfileReturnedFromSeedData()
    {
        var response = await _httpClient.GetAsync("api/profiles");
        await response.AssertSuccessfulStatusCode();

        var returnedProfilesString = await response.Content.ReadAsStringAsync();
        var returnedProfiles = JsonSerializer.Deserialize<List<Profile>>(returnedProfilesString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.Equal(new List<Profile>
        {
            new()
            {
                Id = _testUserId,
                DisplayName = "Default Profile"
            }
        }, returnedProfiles);
    }
}