using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MoneyMateApi.Controllers.Profiles.Dtos;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.IntegrationTests.Extensions;
using MoneyMateApi.IntegrationTests.Helpers;
using MoneyMateApi.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace MoneyMateApi.IntegrationTests.ProfilesEndpoint;

[Collection("IntegrationTests")]
public class ProfilesEndpointTests : IAsyncLifetime
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
        var returnedProfiles = JsonSerializer.Deserialize<List<Profile>>(returnedProfilesString,
            new JsonSerializerOptions
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

    [Fact]
    public async Task GivenCreateProfileRequest_ThenProfileIsFoundInDb()
    {
        const string expectedProfileName = "new profile 123";

        var inputDto = new CreateProfileDto
        {
            DisplayName = expectedProfileName
        };
        var response =
            await _httpClient.PostAsync("/api/profiles",
                new StringContent(JsonSerializer.Serialize(inputDto), Encoding.UTF8, MediaTypeNames.Application.Json));

        await response.AssertSuccessfulStatusCode();

        var storedProfiles = await _cockroachDbIntegrationTestHelper.UserProfileOperations.RetrieveProfiles();

        Assert.Collection(storedProfiles.OrderBy(profile => profile.DisplayName),
            profile => Assert.Equal("Default Profile", profile.DisplayName),
            profile => Assert.Equal(expectedProfileName, profile.DisplayName));
    }
}