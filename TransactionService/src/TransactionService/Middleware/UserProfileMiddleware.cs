using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TransactionService.Constants;
using TransactionService.Domain.Services.Profiles;
using TransactionService.Middleware.Exceptions;

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
            IProfileService profileService)
        {
            var profileIdHeaderFound =
                httpContext.Request.Headers.TryGetValue(Headers.ProfileId, out var profileId);

            // TODO: this will be here temporarily
            if (!profileIdHeaderFound)
            {
                var profiles = await profileService.GetProfiles();
                userContext.ProfileId = profiles.First().Id;
            }
            else
            {
                var profileIdIsValidGuid = Guid.TryParse(profileId, out var profileIdGuid);

                if (!profileIdIsValidGuid)
                    throw new InvalidProfileIdException($"{Headers.ProfileId} is not a valid guid");

                if (!await profileService.VerifyProfileBelongsToCurrentUser(profileIdGuid))
                    throw new ProfileIdForbiddenException(
                        $"User: {userContext.UserId} does not have permissions to access Profile: {profileIdGuid}");

                userContext.ProfileId = profileIdGuid;
            }

            await _next(httpContext);
        }
    }
}