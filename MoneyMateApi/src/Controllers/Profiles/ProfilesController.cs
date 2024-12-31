using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MoneyMateApi.Controllers.Profiles.Dtos;
using MoneyMateApi.Domain.Services.Profiles;

namespace MoneyMateApi.Controllers.Profiles
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfilesController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var profiles = await _profileService.GetProfiles();
            return Ok(profiles);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProfileDto createProfileDto)
        {
            await _profileService.CreateProfile(createProfileDto.DisplayName, false);

            return NoContent();
        }
    }
}