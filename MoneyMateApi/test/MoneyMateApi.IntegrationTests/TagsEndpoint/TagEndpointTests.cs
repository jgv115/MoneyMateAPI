using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MoneyMateApi.Controllers.Tags.Dtos;
using MoneyMateApi.IntegrationTests.Extensions;
using MoneyMateApi.IntegrationTests.Helpers;
using MoneyMateApi.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace MoneyMateApi.IntegrationTests.TagsEndpoint;

[Collection("IntegrationTests")]
public class TagEndpointTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly Guid _testUserId;

    public TagEndpointTests(MoneyMateApiWebApplicationFactory factory)
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
    public async Task GivenCreateTagRequest_ThenTagIsFoundInDb()
    {
        const string tagName = "tagName";

        var inputDto = new CreateTagDto
        {
            Name = tagName
        };

        var response = await _httpClient.PostAsync("/api/tags",
            new StringContent(JsonSerializer.Serialize(inputDto), Encoding.UTF8, MediaTypeNames.Application.Json));

        await response.AssertSuccessfulStatusCode();

        var storedTags = await _cockroachDbIntegrationTestHelper.TagOperations.GetTagsForProfile();

        Assert.Collection(storedTags, tag =>
        {
            Assert.Equal(tagName, tag.Name);
            Assert.Equal(_testUserId, tag.ProfileId);
            Assert.IsType<Guid>(tag.Id);
        });
    }
}