using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TransactionService.Constants;
using TransactionService.Middleware.Exceptions;
using TransactionService.Repositories;

namespace TransactionService.Middleware
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, CurrentUserContext userContext,
            IProfilesRepository profilesRepository)
        {
            var identifierClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (identifierClaim == null)
            {
                throw new ArgumentNullException(nameof(identifierClaim));
            }

            userContext.UserId = identifierClaim.Value;

            var profileIdHeaderFound =
                httpContext.Request.Headers.TryGetValue(Headers.ProfileId, out var profileId);

            if (!profileIdHeaderFound)
            {
                var profiles = await profilesRepository.GetProfiles();
                userContext.ProfileId = profiles.First().Id;
            }
            else
            {
                var profileIdIsValidGuid = Guid.TryParse(profileId, out var profileIdGuid);

                if (!profileIdIsValidGuid)
                    throw new InvalidProfileIdException($"{Headers.ProfileId} is not a valid guid");

                userContext.ProfileId = profileIdGuid;
            }

            await _next(httpContext);
        }
    }
}