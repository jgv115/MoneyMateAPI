using System.Threading.Tasks;
using TransactionService.Middleware;
using TransactionService.Repositories;

namespace TransactionService.Services;

public class InitialisationService : IInitialisationService
{
    private readonly CurrentUserContext _userContext;
    private readonly IProfilesRepository _profilesRepository;

    public InitialisationService(CurrentUserContext userContext, IProfilesRepository profilesRepository)
    {
        _userContext = userContext;
        _profilesRepository = profilesRepository;
    }

    public async Task Initialise()
    {
        await _profilesRepository.CreateProfile(Constants.Defaults.DefaultProfileName);
    }
}