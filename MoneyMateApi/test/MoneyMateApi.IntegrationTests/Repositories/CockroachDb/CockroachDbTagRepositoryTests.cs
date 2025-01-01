using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.IntegrationTests.Helpers;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories.CockroachDb;
using MoneyMateApi.Repositories.CockroachDb.Entities;
using MoneyMateApi.Repositories.Exceptions;
using Xunit;
using Profile = MoneyMateApi.Domain.Models.Profile;

namespace MoneyMateApi.IntegrationTests.Repositories.CockroachDb;

[Collection("IntegrationTests")]
public class CockroachDbTagRepositoryTests : IAsyncLifetime
{
    private readonly DapperContext _dapperContext;
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly Guid _profileId;
    private readonly IMapper _stubMapper;


    public CockroachDbTagRepositoryTests()
    {
        _profileId = Guid.NewGuid();
        _cockroachDbIntegrationTestHelper = new CockroachDbIntegrationTestHelper(_profileId);
        _dapperContext = _cockroachDbIntegrationTestHelper.DapperContext;
        _stubMapper =
            new MapperConfiguration(cfg =>
                    cfg.AddMaps(typeof(CockroachDbPayerPayeeRepository)))
                .CreateMapper();
    }

    [Fact]
    public async Task GivenProfileId_WhenGetTagsInvoked_ThenCorrectTagsReturned()
    {
        // Arrange
        var tagRepository = new CockroachDbTagRepository(_dapperContext, _stubMapper,
            new CurrentUserContext { UserId = _profileId.ToString(), ProfileId = _profileId });

        var insertedTags = await _cockroachDbIntegrationTestHelper.TagOperations.WriteTagsIntoDb(new List<string>
            { "tag1", "tag2", "tag3" });

        // Act
        var tags = await tagRepository.GetTags();

        insertedTags.Sort((x, y) => string.Compare(x.Id, y.Id, StringComparison.Ordinal));
        tags.Sort((x, y) => string.Compare(x.Id, y.Id, StringComparison.Ordinal));
        // Assert
        Assert.Equal(insertedTags, tags);
    }

    [Fact]
    public async Task GivenProfileIdAndTagId_WhenGetTagInvoked_ThenCorrectTagReturned()
    {
        // Arrange
        var tagRepository = new CockroachDbTagRepository(_dapperContext, _stubMapper,
            new CurrentUserContext { UserId = _profileId.ToString(), ProfileId = _profileId });

        var insertedTags = await _cockroachDbIntegrationTestHelper.TagOperations.WriteTagsIntoDb(new List<string>
            { "tag1", "tag2", "tag3" });

        // Act
        var returnedTag = await tagRepository.GetTag(insertedTags[0].Id);

        // Assert
        Assert.Equal(insertedTags[0], returnedTag);
    }

    [Fact]
    public async Task GivenNewTagName_WhenCreateTagInvoked_ThenTagStoredInDbAndTagObjectReturned()
    {
        // Arrange
        var tagRepository = new CockroachDbTagRepository(_dapperContext, _stubMapper,
            new CurrentUserContext { UserId = _profileId.ToString(), ProfileId = _profileId });

        // Act
        var storedTag = await tagRepository.CreateTag("tag_name");

        // Assert
        var retrievedTags = await _cockroachDbIntegrationTestHelper.TagOperations.GetTagsForProfile();

        Assert.Single(retrievedTags);
        Assert.Collection(retrievedTags, tag =>
        {
            Assert.Equal(storedTag.Id, tag.Id.ToString());

            // Ensure tag name is saved correctly
            Assert.Equal(storedTag.Name, tag.Name);
            Assert.Equal("tag_name", tag.Name);

            // Ensure profileId is saved correctly
            Assert.Equal(_profileId, tag.ProfileId);

            // Calculate that the date saved is within a minute of the current time
            var timeDifference = DateTime.UtcNow - tag.CreatedAt;
            Assert.True(timeDifference < TimeSpan.FromMinutes(1));
        });
    }

    [Fact]
    public async Task GivenTagNameAlreadyExists_WhenCreateTagInvoked_ThenRepositoryItemExistsExceptionThrown()
    {
        // Arrange
        var tagRepository = new CockroachDbTagRepository(_dapperContext, _stubMapper,
            new CurrentUserContext { UserId = _profileId.ToString(), ProfileId = _profileId });

        // Act & Assert
        var tagName = "duplicate_tag_name";
        await tagRepository.CreateTag(tagName);
        await Assert.ThrowsAsync<RepositoryItemExistsException>(() => tagRepository.CreateTag(tagName));
    }

    [Fact]
    public async Task
        GivenSameTagNameForDifferentProfileIds_WhenCreateTagInvoked_ThenRepositoryItemExistsExceptionThrown()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // create new user
        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteUsersIntoDb(new List<User>
        {
            new()
            {
                Id = userId,
                UserIdentifier = userId.ToString()
            }
        });
        
        // create two profiles for user
        var profileId1 = Guid.NewGuid();
        var profileId2 = Guid.NewGuid();

        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteProfilesIntoDbForUser(new List<Profile>
        {
            new()
            {
                DisplayName = "Profile 1",
                Id = profileId1
            },
            new()
            {
                DisplayName = "Profile 2",
                Id = profileId2
            }
        }, userId);

        // write tags for both profiles with the same name
        var tagRepository1 = new CockroachDbTagRepository(_dapperContext, _stubMapper,
            new CurrentUserContext { UserId = profileId1.ToString(), ProfileId = profileId1 });

        var tagRepository2 = new CockroachDbTagRepository(_dapperContext, _stubMapper,
            new CurrentUserContext { UserId = profileId2.ToString(), ProfileId = profileId2 });

        var tagName = "duplicate_tag_name";
        var tasks = new List<Task> { tagRepository1.CreateTag(tagName), tagRepository2.CreateTag(tagName) };

        // Act
        await Task.WhenAll(tasks);

        // Assert
        var retrievedTags = await _cockroachDbIntegrationTestHelper.TagOperations.GetAllTagsFromDb();

        Assert.Equal(2, retrievedTags.Count);
    }


    public async Task InitializeAsync()
    {
        await _cockroachDbIntegrationTestHelper.SeedRequiredData();
    }

    public async Task DisposeAsync()
    {
        await _cockroachDbIntegrationTestHelper.ClearDbData();
    }
}