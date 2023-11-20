using Microsoft.AspNetCore.Mvc;

namespace TransactionService.Controllers.Initialisation;

[ApiController]
[Route("api/[controller]")]
public class InitialisationController : ControllerBase
{
    [HttpPost]
    public IActionResult InitialiseProfile()
    {
        return NoContent();
    }
}