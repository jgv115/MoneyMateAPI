using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TransactionService.Constants;
using TransactionService.Middleware;
using TransactionService.Middleware.Exceptions;
using Xunit;

namespace TransactionService.Tests.Middleware
{
    public class UserContextMiddlewareTests
    {
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
            await sut.Invoke(httpContext, currentUserContext);

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
            await sut.Invoke(httpContext, currentUserContext);

            Assert.Equal(profileId, currentUserContext.ProfileId);
        }
        
        [Fact]
        public async void GivenMissingMoneyMateProfileHeader_ThenInvalidProfileIdExceptionThrown()
        {
            var userId = "userId123";
            var currentUserContext = new CurrentUserContext();

            var httpContext = new DefaultHttpContext();
            var identity = new ClaimsIdentity("ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            httpContext.User = new ClaimsPrincipal(identity);

            var sut = new UserContextMiddleware(async _ => await Task.Delay(0));

            await Assert.ThrowsAsync<InvalidProfileIdException>(() => sut.Invoke(httpContext, currentUserContext));
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

            await Assert.ThrowsAsync<InvalidProfileIdException>(() => sut.Invoke(httpContext, currentUserContext));
        }

        [Fact]
        public async void InvokeAsync_ShouldThrowArgumentNullException_WhenIdentifierClaimIsNull()
        {
            var sut = new UserContextMiddleware(async _ => await Task.Delay(0));
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.Invoke(new DefaultHttpContext(), new CurrentUserContext()));
        }
    }
}