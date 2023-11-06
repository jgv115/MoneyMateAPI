using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TransactionService.Constants;
using TransactionService.Middleware.Exceptions;
using TransactionService.Repositories;

namespace TransactionService.Middleware
{
    public class UserProfileMiddleware
    {
        private readonly RequestDelegate _next;

        public UserProfileMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, CurrentUserContext userContext,
            IProfilesRepository profilesRepository)
        {
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