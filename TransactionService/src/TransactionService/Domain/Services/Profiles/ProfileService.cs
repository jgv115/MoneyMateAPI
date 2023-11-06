using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.Repositories;

namespace TransactionService.Domain.Services.Profiles;

public class ProfileService : IProfileService
{
    private readonly IProfilesRepository _profilesRepository;

    public ProfileService(IProfilesRepository profilesRepository)
    {
        _profilesRepository = profilesRepository;
    }

    public Task<List<Profile>> GetProfiles()
    {
        return _profilesRepository.GetProfiles();
    }

    public Task CreateProfile(string profileDisplayName)
    {
        return _profilesRepository.CreateProfile(profileDisplayName);
    }
}