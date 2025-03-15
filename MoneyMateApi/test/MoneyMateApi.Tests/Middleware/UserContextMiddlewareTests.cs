using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MoneyMateApi.Middleware;
using MoneyMateApi.Tests.Middleware.Helpers;
using Xunit;

namespace MoneyMateApi.Tests.Middleware
{
    public class UserContextMiddlewareTests
    {
        [Fact]
        public async Task GivenUserIdInHttpContext_ThenUserIdSetInCurrentUserContext()
        {
            var userId = "userId123";
            var profileId = Guid.NewGuid();
            var currentUserContext = new CurrentUserContext();

            var httpContext = new TestHttpContextBuilder().WithUserId(userId)
                .WithMoneyMateProfileHeader(profileId.ToString()).Build();

            var sut = new UserContextMiddleware(async _ => await Task.Delay(0));
            await sut.Invoke(httpContext, currentUserContext);

            Assert.Equal(userId, currentUserContext.UserId);
        }

        [Fact]
        public async Task InvokeAsync_ShouldThrowArgumentNullException_WhenIdentifierClaimIsNull()
        {
            var sut = new UserContextMiddleware(async _ => await Task.Delay(0));
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.Invoke(new DefaultHttpContext(), new CurrentUserContext()));
        }
    }
}