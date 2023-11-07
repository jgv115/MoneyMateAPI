using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;

namespace TransactionService.Repositories;

public interface IProfilesRepository
{
    public Task<Profile> GetProfile(Guid profileId);
    public Task<List<Profile>> GetProfiles();
    public Task CreateProfile(string displayName);
}