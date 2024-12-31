using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MoneyMateApi.Constants;
using MoneyMateApi.Domain.Services.Profiles;
using MoneyMateApi.Middleware.Exceptions;

namespace MoneyMateApi.Middleware
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

            if (!profileIdHeaderFound)
                throw new InvalidProfileIdException($"{Headers.ProfileId} was not set");
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