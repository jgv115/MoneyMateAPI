using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MoneyMateApi.Controllers.Profiles;
using MoneyMateApi.Controllers.Profiles.Dtos;
using MoneyMateApi.Domain.Profiles;
using Xunit;

namespace MoneyMateApi.Tests.Controllers.Profiles;

public class ProfilesControllerTests
{
    private readonly Mock<IProfileService> _mockProfileService = new();

    [Fact]
    public async Task GivenServiceReturnsProfiles_WhenGetInvoked_ThenOkReturnedWithProfiles()
    {
        var controller = new ProfilesController(_mockProfileService.Object);

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

        _mockProfileService.Setup(service => service.GetProfiles()).ReturnsAsync(expectedProfiles);
        var response = await controller.Get();

        var responseObject = Assert.IsType<OkObjectResult>(response);

        var returnedProfiles = responseObject.Value as List<Profile>;

        Assert.Equal(expectedProfiles, returnedProfiles);
    }

    [Fact]
    public async Task GivenServiceSuccessful_WhenCreateInvoked_ThenNoContentReturned()
    {
        var controller = new ProfilesController(_mockProfileService.Object);

        const string expectedNewProfileName = "new Profile 123";
        _mockProfileService.Setup(service => service.CreateProfile(expectedNewProfileName, false)).Verifiable();

        var response = await controller.Create(new CreateProfileDto
        {
            DisplayName = expectedNewProfileName
        });

        Assert.IsType<NoContentResult>(response);

        _mockProfileService.Verify(service => service.CreateProfile(expectedNewProfileName, false), Times.Once);
    }
}