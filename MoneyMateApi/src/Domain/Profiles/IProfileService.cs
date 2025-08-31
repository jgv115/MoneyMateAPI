using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyMateApi.Domain.Profiles;

public interface IProfileService
{
    public Task<IEnumerable<Profile>> GetProfiles();
    public Task<bool> VerifyProfileBelongsToCurrentUser(Guid profileIdToBeVerified);
    // Returns the UUID of the created profile
    public Task<Guid> CreateProfile(string profileDisplayName, bool initialiseCategories);
}