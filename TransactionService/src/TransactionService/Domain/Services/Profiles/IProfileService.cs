using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;

namespace TransactionService.Domain.Services.Profiles;

public interface IProfileService
{
    public Task<List<Profile>> GetProfiles();
    public Task CreateProfile(string profileDisplayName);
}