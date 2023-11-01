using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TransactionService.Controllers.Profiles;
using TransactionService.Domain.Models;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Controllers.Profiles;

public class ProfilesControllerTests
{
    private readonly Mock<IProfilesRepository> _mockProfilesRepository = new();

    [Fact]
    public async Task GivenRepositoryReturnsProfiles_WhenGetInvoked_ThenOkReturnedWithProfiles()
    {
        var controller = new ProfilesController(_mockProfilesRepository.Object);

        var expectedProfiles = new List<Profile>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Default Profile"
            },
            new()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Second Profile"
            }
        };

        _mockProfilesRepository.Setup(repository => repository.GetProfiles()).ReturnsAsync(expectedProfiles);
        var response = await controller.Get();

        var responseObject = Assert.IsType<OkObjectResult>(response);

        var returnedProfiles = responseObject.Value as List<Profile>;
        
        Assert.Equal(expectedProfiles, returnedProfiles);
    }
}