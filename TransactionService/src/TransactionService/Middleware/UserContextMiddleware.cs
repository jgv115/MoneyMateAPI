using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TransactionService.Middleware
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, CurrentUserContext userContext)
        {
            var identifierClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (identifierClaim == null)
            {
                throw new ArgumentNullException(nameof(identifierClaim));
            }

            userContext.UserId = identifierClaim.Value;

            await _next(httpContext);
        }
    }
}