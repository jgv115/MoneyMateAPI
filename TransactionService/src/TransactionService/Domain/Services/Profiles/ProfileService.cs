using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Models;
using TransactionService.Middleware;
using TransactionService.Repositories;

namespace TransactionService.Domain.Services.Profiles;

public class ProfileService : IProfileService
{
    private readonly ILogger<ProfileService> _logger;
    private readonly CurrentUserContext _userContext;
    private readonly IProfilesRepository _profilesRepository;

    public ProfileService(ILogger<ProfileService> logger, IProfilesRepository profilesRepository,
        CurrentUserContext userContext)
    {
        _logger = logger;
        _profilesRepository = profilesRepository;
        _userContext = userContext;
    }

    public Task<List<Profile>> GetProfiles()
    {
        return _profilesRepository.GetProfiles();
    }

    public async Task<bool> VerifyProfileBelongsToCurrentUser(Guid profileIdToBeVerified)
    {
        try
        {
            await _profilesRepository.GetProfile(profileIdToBeVerified);
            return true;
        }
        catch
        {
            _logger.LogWarning(
                "User with user identifier {UserIdentifier} has unsuccessfully tried to access profile with Id: {ProfileId}",
                _userContext.UserId, profileIdToBeVerified);
            return false;
        }
    }

    public Task CreateProfile(string profileDisplayName)
    {
        return _profilesRepository.CreateProfile(profileDisplayName);
    }
}