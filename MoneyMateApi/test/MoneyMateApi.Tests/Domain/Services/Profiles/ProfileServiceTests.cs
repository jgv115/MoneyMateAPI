using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoneyMateApi.Domain.Profiles;
using Moq;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories;
using MoneyMateApi.Repositories.CockroachDb.Entities;
using MoneyMateApi.Repositories.Exceptions;
using MoneyMateApi.Services.Initialisation.CategoryInitialisation;
using Xunit;

namespace MoneyMateApi.Tests.Domain.Services.Profiles;

public class ProfileServiceTests
{
    private readonly Mock<IProfilesRepository> _mockProfilesRepository = new();
    private readonly Mock<ILogger<ProfileService>> _mockLogger = new();

    private readonly CurrentUserContext _stubContext = new CurrentUserContext
    {
        UserId = "test-user-123"
    };

    private readonly Mock<ICategoryInitialiser> _mockCategoryInitialiser = new();
    private readonly Mock<IUserRepository> _mockUserRepository = new();

    [Fact]
    public async Task WhenGetProfilesInvoked_ThenRepositoryResponseReturned()
    {
        var service = new ProfileService(_mockLogger.Object, _mockProfilesRepository.Object, _stubContext,
            _mockCategoryInitialiser.Object, _mockUserRepository.Object);

        var expectedProfiles = new List<Profile>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DisplayName = "test"
            },
            new()
            {
                Id = Guid.NewGuid(),
                DisplayName = "test"
            }
        };

        _mockProfilesRepository.Setup(repository => repository.GetProfiles()).ReturnsAsync(expectedProfiles);

        var response = await service.GetProfiles();

        Assert.Equal(expectedProfiles, response);
    }

    [Fact]
    public async Task
        GivenRepositoryThrowsErrorWhenGettingProfile_WhenVerifyProfileBelongsToCurrentUserInvoked_ThenFalseReturned()
    {
        var service = new ProfileService(_mockLogger.Object, _mockProfilesRepository.Object, _stubContext,
            _mockCategoryInitialiser.Object, _mockUserRepository.Object);

        var idToBeVerified = Guid.NewGuid();

        _mockProfilesRepository.Setup(repository => repository.GetProfile(idToBeVerified))
            .ThrowsAsync(new RepositoryItemDoesNotExist("error"));

        var result = await service.VerifyProfileBelongsToCurrentUser(idToBeVerified);

        Assert.False(result);
    }

    [Fact]
    public async Task GivenRepositoryReturnsProfile_WhenVerifyProfileBelongsToCurrentUserInvoked_ThenTrueReturned()
    {
        var service = new ProfileService(_mockLogger.Object, _mockProfilesRepository.Object, _stubContext,
            _mockCategoryInitialiser.Object, _mockUserRepository.Object);

        var idToBeVerified = Guid.NewGuid();
        _mockProfilesRepository.Setup(repository => repository.GetProfile(idToBeVerified))
            .ReturnsAsync(() => new Profile());

        var result = await service.VerifyProfileBelongsToCurrentUser(idToBeVerified);

        Assert.True(result);
    }

    [Fact]
    public async Task
        GivenDisplayNameAndInitialiseCategoriesIsFalse_WhenCreateProfileInvoked_ThenIdOfProfileCreatedIsReturned()
    {
        var service = new ProfileService(_mockLogger.Object, _mockProfilesRepository.Object, _stubContext,
            _mockCategoryInitialiser.Object, _mockUserRepository.Object);

        const string expectedProfileName = "new profile 123";
        var expectedProfileId = Guid.NewGuid();
        _mockProfilesRepository.Setup(repository => repository.CreateProfile(expectedProfileName))
            .ReturnsAsync(expectedProfileId);

        var createdProfileId = await service.CreateProfile(expectedProfileName);

        Assert.Equal(expectedProfileId, createdProfileId);
    }

    [Fact]
    public async Task
        GivenDisplayNameAndInitialiseCategoriesIsTrue_WhenCreateProfileInvoked_ThenProfileCreatedInRepositoryAndCategoriesAreInitialised()
    {
        var service = new ProfileService(_mockLogger.Object, _mockProfilesRepository.Object, _stubContext,
            _mockCategoryInitialiser.Object, _mockUserRepository.Object);

        const string expectedProfileName = "new profile 123";
        var expectedProfileId = Guid.NewGuid();
        _mockProfilesRepository.Setup(repository => repository.CreateProfile(expectedProfileName))
            .ReturnsAsync(expectedProfileId);

        var expectedReturnedUser = new User
        {
            Id = Guid.NewGuid(),
            UserIdentifier = "id1234"
        };
        _mockUserRepository.Setup(repository => repository.GetUser()).ReturnsAsync(expectedReturnedUser);

        var createdProfileId = await service.CreateProfile(expectedProfileName, initialiseCategories: true);

        Assert.Equal(expectedProfileId, createdProfileId);
        _mockCategoryInitialiser.Verify(initialiser =>
            initialiser.InitialiseCategories(expectedReturnedUser.Id, expectedProfileId));
    }
}