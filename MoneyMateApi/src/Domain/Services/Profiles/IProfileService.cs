using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.Domain.Models;

namespace MoneyMateApi.Domain.Services.Profiles;

public interface IProfileService
{
    public Task<List<Profile>> GetProfiles();
    public Task<bool> VerifyProfileBelongsToCurrentUser(Guid profileIdToBeVerified);
    // Returns the UUID of the created profile
    public Task<Guid> CreateProfile(string profileDisplayName, bool initialiseCategories);
}