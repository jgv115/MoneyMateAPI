using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Services;

namespace TransactionService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategoryBreakdown([FromQuery] string type, [FromQuery] int? count = null,
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null)
        {
            var analyticsCategories = await _analyticsService.GetCategoryBreakdown(type, count,
                start.GetValueOrDefault(DateTime.MinValue),
                end.GetValueOrDefault(DateTime.MaxValue));
            return Ok(analyticsCategories);
        }
    }
}