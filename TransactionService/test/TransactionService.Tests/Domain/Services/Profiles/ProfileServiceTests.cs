using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Profiles;
using TransactionService.Middleware;
using TransactionService.Repositories;
using TransactionService.Repositories.Exceptions;
using Xunit;

namespace TransactionService.Tests.Domain.Services.Profiles;

public class ProfileServiceTests
{
    private readonly Mock<IProfilesRepository> _mockProfilesRepository = new();
    private readonly Mock<ILogger<ProfileService>> _mockLogger = new();

    private readonly CurrentUserContext _stubContext = new CurrentUserContext
    {
        UserId = "test-user-123"
    };

    [Fact]
    public async Task WhenGetProfilesInvoked_ThenRepositoryResponseReturned()
    {
        var service = new ProfileService(_mockLogger.Object, _mockProfilesRepository.Object, _stubContext);

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
        var service = new ProfileService(_mockLogger.Object, _mockProfilesRepository.Object, _stubContext);

        var idToBeVerified = Guid.NewGuid();

        _mockProfilesRepository.Setup(repository => repository.GetProfile(idToBeVerified))
            .ThrowsAsync(new RepositoryItemDoesNotExist("error"));

        var result = await service.VerifyProfileBelongsToCurrentUser(idToBeVerified);

        Assert.False(result);
    }

    [Fact]
    public async Task GivenRepositoryReturnsProfile_WhenVerifyProfileBelongsToCurrentUserInvoked_ThenTrueReturned()
    {
        var service = new ProfileService(_mockLogger.Object, _mockProfilesRepository.Object, _stubContext);

        var idToBeVerified = Guid.NewGuid();
        _mockProfilesRepository.Setup(repository => repository.GetProfile(idToBeVerified))
            .ReturnsAsync(() => new Profile());

        var result = await service.VerifyProfileBelongsToCurrentUser(idToBeVerified);

        Assert.True(result);
    }

    [Fact]
    public async Task GivenDisplayName_WhenCreateProfileInvoked_ThenProfileCreatedInRepository()
    {
        var service = new ProfileService(_mockLogger.Object, _mockProfilesRepository.Object, _stubContext);

        const string expectedProfileName = "new profile 123";
        _mockProfilesRepository.Setup(repository => repository.CreateProfile(expectedProfileName)).Verifiable();

        await service.CreateProfile(expectedProfileName);

        _mockProfilesRepository.Verify(repository => repository.CreateProfile(expectedProfileName), Times.Once);
    }
}