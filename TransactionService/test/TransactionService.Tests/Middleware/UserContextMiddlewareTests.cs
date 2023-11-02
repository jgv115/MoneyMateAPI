using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Middleware;
using TransactionService.Middleware.Exceptions;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Middleware
{
    public class UserContextMiddlewareTests
    {
        private readonly Mock<IProfilesRepository> _profilesRepository = new();

        [Fact]
        public async void GivenUserIdInHttpContext_ThenUserIdSetInCurrentUserContext()
        {
            var userId = "userId123";
            var profileId = Guid.NewGuid();
            var currentUserContext = new CurrentUserContext();

            var httpContext = new DefaultHttpContext();
            var identity = new ClaimsIdentity("ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            httpContext.User = new ClaimsPrincipal(identity);
            httpContext.Request.Headers["MoneyMate-Profile-Id"] = profileId.ToString();

            var sut = new UserContextMiddleware(async _ => await Task.Delay(0));
            await sut.Invoke(httpContext, currentUserContext, _profilesRepository.Object);

            Assert.Equal(userId, currentUserContext.UserId);
        }

        [Fact]
        public async void GivenMoneyMateProfileHeader_ThenProfileIdSetInCurrentUserContext()
        {
            var userId = "userId123";
            var profileId = Guid.NewGuid();
            var currentUserContext = new CurrentUserContext();

            var httpContext = new DefaultHttpContext();
            var identity = new ClaimsIdentity("ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            httpContext.User = new ClaimsPrincipal(identity);
            httpContext.Request.Headers["MoneyMate-Profile-Id"] = profileId.ToString();

            var sut = new UserContextMiddleware(async _ => await Task.Delay(0));
            await sut.Invoke(httpContext, currentUserContext, _profilesRepository.Object);

            Assert.Equal(profileId, currentUserContext.ProfileId);
        }

        [Fact]
        public async void GivenMissingMoneyMateProfileHeader_ThenProfileIdSetToFirstProfileReturnedFromRepository()
        {
            var userId = "userId123";
            var currentUserContext = new CurrentUserContext();

            var httpContext = new DefaultHttpContext();
            var identity = new ClaimsIdentity("ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            httpContext.User = new ClaimsPrincipal(identity);

            var sut = new UserContextMiddleware(async _ => await Task.Delay(0));

            var profileId = Guid.NewGuid();
            _profilesRepository.Setup(repository => repository.GetProfiles()).ReturnsAsync(new List<Profile>
            {
                new()
                {
                    Id = profileId,
                    DisplayName = "Default Profile"
                }
            });

            await sut.Invoke(httpContext, currentUserContext, _profilesRepository.Object);
            
            Assert.Equal(profileId, currentUserContext.ProfileId);
        }

        [Fact]
        public async void GivenInvalidMateProfileHeader_ThenInvalidProfileIdExceptionThrown()
        {
            var userId = "userId123";
            var currentUserContext = new CurrentUserContext();

            var httpContext = new DefaultHttpContext();
            var identity = new ClaimsIdentity("ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            httpContext.User = new ClaimsPrincipal(identity);
            httpContext.Request.Headers[Headers.ProfileId] = "invalid";

            var sut = new UserContextMiddleware(async _ => await Task.Delay(0));

            await Assert.ThrowsAsync<InvalidProfileIdException>(() =>
                sut.Invoke(httpContext, currentUserContext, _profilesRepository.Object));
        }

        [Fact]
        public async void InvokeAsync_ShouldThrowArgumentNullException_WhenIdentifierClaimIsNull()
        {
            var sut = new UserContextMiddleware(async _ => await Task.Delay(0));
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.Invoke(new DefaultHttpContext(), new CurrentUserContext(), _profilesRepository.Object));
        }
    }
}