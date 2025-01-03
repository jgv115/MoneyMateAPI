using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MoneyMateApi.IntegrationTests.Helpers;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories.CockroachDb;
using MoneyMateApi.Repositories.CockroachDb.Entities;
using MoneyMateApi.Repositories.Exceptions;
using Xunit;
using Profile = MoneyMateApi.Domain.Models.Profile;
using Tag = MoneyMateApi.Domain.Models.Tag;

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

        var tag1 = new Tag(Guid.NewGuid(), "tag1");
        var tag2 = new Tag(Guid.NewGuid(), "tag2");
        var tag3 = new Tag(Guid.NewGuid(), "tag3");
        var insertedTags = await _cockroachDbIntegrationTestHelper.TagOperations.WriteTagsIntoDb([tag1, tag2, tag3]);

        // Act
        var tags = (await tagRepository.GetTags()).ToList();

        insertedTags.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
        tags.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
        // Assert
        Assert.Equal(insertedTags, tags);
    }

    [Fact]
    public async Task GivenListOfTagIds_WhenGetTagsInvoked_ThenCorrectTagsReturned()
    {
        // Arrange
        var tagRepository = new CockroachDbTagRepository(_dapperContext, _stubMapper,
            new CurrentUserContext { UserId = _profileId.ToString(), ProfileId = _profileId });

        var tag1 = new Tag(Guid.Parse("c6eae8c1-2514-4e21-9841-785db172ee35"), "tag1");
        var tag2 = new Tag(Guid.Parse("c6eae8c1-2514-4e21-9841-785db172ee36"), "tag2");
        var tag3 = new Tag(Guid.Parse("c6eae8c1-2514-4e21-9841-785db172ee37"), "tag3");
        await _cockroachDbIntegrationTestHelper.TagOperations.WriteTagsIntoDb([tag1, tag2, tag3]);

        // Act
        var tags = await tagRepository.GetTags([tag1.Id, tag3.Id]);

        // Assert
        Assert.Equal(new Dictionary<Guid, Tag>
        {
            { tag1.Id, tag1 },
            { tag3.Id, tag3 }
        }, tags);
    }
    
    [Fact]
    public async Task GivenProfileIdAndTagId_WhenGetTagInvoked_ThenCorrectTagReturned()
    {
        // Arrange
        var tagRepository = new CockroachDbTagRepository(_dapperContext, _stubMapper,
            new CurrentUserContext { UserId = _profileId.ToString(), ProfileId = _profileId });

        var tag1 = new Tag(Guid.NewGuid(), "tag1");
        var tag2 = new Tag(Guid.NewGuid(), "tag2");
        var tag3 = new Tag(Guid.NewGuid(), "tag3");
        var insertedTags = await _cockroachDbIntegrationTestHelper.TagOperations.WriteTagsIntoDb([tag1, tag2, tag3]);

        // Act
        var returnedTag = await tagRepository.GetTag(tag1.Id);

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
            Assert.Equal(storedTag.Id, tag.Id);

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