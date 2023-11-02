using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Repositories;

namespace TransactionService.Controllers.Profiles
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfilesRepository _profilesRepository;

        public ProfilesController(IProfilesRepository profilesRepository)
        {
            _profilesRepository = profilesRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var profiles = await _profilesRepository.GetProfiles();
            return Ok(profiles);
        }
    }
}