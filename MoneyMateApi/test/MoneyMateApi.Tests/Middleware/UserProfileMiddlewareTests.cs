using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MoneyMateApi.Tests.Middleware.Helpers;
using Moq;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Profiles;
using MoneyMateApi.Middleware;
using MoneyMateApi.Middleware.Exceptions;
using Xunit;

namespace MoneyMateApi.Tests.Middleware;

public class UserProfileMiddlewareTests
{
    private readonly Mock<IProfileService> _mockProfileService = new();

    [Fact]
    public async Task GivenMoneyMateProfileHeaderAndProfileBelongsToUser_ThenProfileIdSetInCurrentUserContext()
    {
        var userId = "userId123";
        var profileId = Guid.NewGuid();
        var currentUserContext = new CurrentUserContext();

        var httpContext = new TestHttpContextBuilder()
            .WithUserId(userId)
            .WithMoneyMateProfileHeader(profileId.ToString())
            .Build();

        var sut = new UserProfileMiddleware(async _ => await Task.Delay(0));

        _mockProfileService.Setup(service => service.VerifyProfileBelongsToCurrentUser(profileId)).ReturnsAsync(true);

        await sut.Invoke(httpContext, currentUserContext, _mockProfileService.Object);

        Assert.Equal(profileId, currentUserContext.ProfileId);
    }

    [Fact]
    public async Task GivenMoneyMateProfileHeaderAndProfileDoesNotBelongToUser_ThenProfileIdForbiddenExceptionThrown()
    {
        var userId = "userId123";
        var profileId = Guid.NewGuid();
        var currentUserContext = new CurrentUserContext();

        var httpContext = new TestHttpContextBuilder()
            .WithUserId(userId)
            .WithMoneyMateProfileHeader(profileId.ToString())
            .Build();

        var sut = new UserProfileMiddleware(async _ => await Task.Delay(0));

        _mockProfileService.Setup(service => service.VerifyProfileBelongsToCurrentUser(profileId)).ReturnsAsync(false);

        await Assert.ThrowsAsync<ProfileIdForbiddenException>(() =>
            sut.Invoke(httpContext, currentUserContext, _mockProfileService.Object));
    }

    [Fact]
    public async Task GivenMissingMoneyMateProfileHeader_ThenInvalidProfileExceptionThrown()
    {
        var userId = "userId123";
        var currentUserContext = new CurrentUserContext();

        var httpContext = new TestHttpContextBuilder().WithUserId(userId).Build();

        var sut = new UserProfileMiddleware(async _ => await Task.Delay(0));

        var profileId = Guid.NewGuid();
        _mockProfileService.Setup(service => service.GetProfiles()).ReturnsAsync(new List<Profile>
        {
            new()
            {
                Id = profileId,
                DisplayName = "Default Profile"
            }
        });

        await Assert.ThrowsAsync<InvalidProfileIdException>(() =>
            sut.Invoke(httpContext, currentUserContext, _mockProfileService.Object));
    }

    [Fact]
    public async Task GivenInvalidMateProfileHeader_ThenInvalidProfileIdExceptionThrown()
    {
        var userId = "userId123";
        var currentUserContext = new CurrentUserContext();

        var httpContext = new TestHttpContextBuilder()
            .WithUserId(userId)
            .WithMoneyMateProfileHeader("invalid")
            .Build();

        var sut = new UserProfileMiddleware(async _ => await Task.Delay(0));

        await Assert.ThrowsAsync<InvalidProfileIdException>(() =>
            sut.Invoke(httpContext, currentUserContext, _mockProfileService.Object));
    }
}