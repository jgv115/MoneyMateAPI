using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Profiles;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Domain.Services.Profiles;

public class ProfileServiceTests
{
    private readonly Mock<IProfilesRepository> _mockProfilesRepository = new();

    [Fact]
    public async Task WhenGetProfilesInvoked_ThenRepositoryResponseReturned()
    {
        var service = new ProfileService(_mockProfilesRepository.Object);

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
    public async Task GivenDisplayName_WhenCreateProfileInvoked_ThenProfileCreatedInRepository()
    {
        var service = new ProfileService(_mockProfilesRepository.Object);

        const string expectedProfileName = "new profile 123";
        _mockProfilesRepository.Setup(repository => repository.CreateProfile(expectedProfileName)).Verifiable();

        await service.CreateProfile(expectedProfileName);

        _mockProfilesRepository.Verify(repository => repository.CreateProfile(expectedProfileName), Times.Once);
    }
}