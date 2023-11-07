using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Profiles;
using TransactionService.Middleware;
using TransactionService.Middleware.Exceptions;
using TransactionService.Tests.Middleware.Helpers;
using Xunit;

namespace TransactionService.Tests.Middleware;

public class UserProfileMiddlewareTests
{
    private readonly Mock<IProfileService> _mockProfileService = new();

    [Fact]
    public async void GivenMoneyMateProfileHeaderAndProfileBelongsToUser_ThenProfileIdSetInCurrentUserContext()
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
    public async void GivenMoneyMateProfileHeaderAndProfileDoesNotBelongToUser_ThenProfileIdForbiddenExceptionThrown()
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
    public async void GivenMissingMoneyMateProfileHeader_ThenProfileIdSetToFirstProfileReturnedFromService()
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

        await sut.Invoke(httpContext, currentUserContext, _mockProfileService.Object);

        Assert.Equal(profileId, currentUserContext.ProfileId);
    }

    [Fact]
    public async void GivenInvalidMateProfileHeader_ThenInvalidProfileIdExceptionThrown()
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

    [Fact]
    public async void InvokeAsync_ShouldThrowArgumentNullException_WhenIdentifierClaimIsNull()
    {
        var sut = new UserProfileMiddleware(async _ => await Task.Delay(0));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.Invoke(new DefaultHttpContext(), new CurrentUserContext(), _mockProfileService.Object));
    }
}