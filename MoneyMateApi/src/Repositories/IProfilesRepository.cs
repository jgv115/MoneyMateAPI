using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.Domain.Profiles;

namespace MoneyMateApi.Repositories;

public interface IProfilesRepository
{
    public Task<Profile> GetProfile(Guid profileId);
    public Task<IEnumerable<Profile>> GetProfiles();
    public Task<Guid> CreateProfile(string displayName);
    public Task<Guid> CreateProfile(string displayName, Guid profileId);
}