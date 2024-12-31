using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MoneyMateApi.Constants;

namespace MoneyMateApi.Tests.Middleware.Helpers;

class TestHttpContextBuilder
{
    private readonly HttpContext _context;

    public TestHttpContextBuilder()
    {
        _context = new DefaultHttpContext();
    }

    public TestHttpContextBuilder WithUserId(string userId)
    {
        var identity = new ClaimsIdentity("ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
            ClaimsIdentity.DefaultRoleClaimType);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        _context.User = new ClaimsPrincipal(identity);

        return this;
    }

    public TestHttpContextBuilder WithMoneyMateProfileHeader(string profileId)
    {
        _context.Request.Headers[Headers.ProfileId] = profileId;
        return this;
    }

    public HttpContext Build()
    {
        return _context;
    }
}