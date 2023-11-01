using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TransactionService.Controllers.Profiles
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController: ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok();
        }
    }
}