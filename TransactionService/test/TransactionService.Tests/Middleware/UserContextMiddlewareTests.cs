using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TransactionService.Middleware;
using TransactionService.Models;
using Xunit;

namespace TransactionService.Tests.Middleware
{
    public class UserContextMiddlewareTests
    {
        [Fact]
        public async void InvokeAsync_ShouldSetUserIdInCurrentUserContext()
        {
            var userId = "userId123";
            var currentUserContext = new CurrentUserContext();

            var httpContext = new DefaultHttpContext();
            var identity = new ClaimsIdentity("ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            httpContext.User = new ClaimsPrincipal(identity);
            
            var sut = new UserContextMiddleware(async _ => await Task.Delay(0));
            await sut.Invoke(httpContext, currentUserContext);
            
            Assert.Equal(userId, currentUserContext.UserId);
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