using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Models;
using TransactionService.Middleware;
using TransactionService.Repositories;
using TransactionService.Services.Initialisation.CategoryInitialisation;

namespace TransactionService.Domain.Services.Profiles;

public class ProfileService : IProfileService
{
    private readonly ILogger<ProfileService> _logger;
    private readonly CurrentUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IProfilesRepository _profilesRepository;
    private readonly ICategoryInitialiser _categoryInitialiser;

    public ProfileService(ILogger<ProfileService> logger,
        IProfilesRepository profilesRepository,
        CurrentUserContext userContext,
        ICategoryInitialiser categoryInitialiser,
        IUserRepository userRepository)
    {
        _logger = logger;
        _profilesRepository = profilesRepository;
        _userContext = userContext;
        _categoryInitialiser = categoryInitialiser;
        _userRepository = userRepository;
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

    public async Task<Guid> CreateProfile(string profileDisplayName, bool initialiseCategories = false)
    {
        var profileId = await _profilesRepository.CreateProfile(profileDisplayName);

        if (initialiseCategories)
        {
            var user = await _userRepository.GetUser();
            await _categoryInitialiser.InitialiseCategories(user.Id, profileId);
        }

        return profileId;
    }
}