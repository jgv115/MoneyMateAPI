using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Services;
using TransactionService.ViewModels;

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

        // TODO: put query params inta class and use fluent validation to validate them
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategoryBreakdown([FromQuery] string type, [FromQuery] int? count = null,
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null,
            [FromQuery] string? frequency = null,
            [FromQuery] int? periods = null)
        {
            IEnumerable<AnalyticsCategory> analyticsCategories;
            if (start.HasValue && end.HasValue)
            {
                analyticsCategories = await _analyticsService.GetCategoryBreakdown(type, count, start.Value, end.Value);
            }
            else if (!string.IsNullOrEmpty(frequency) && periods.HasValue)
            {
                analyticsCategories = await _analyticsService.GetCategoryBreakdown(type, count, new TimePeriod(frequency, periods.Value));
            }
            else
            {
                analyticsCategories = await _analyticsService.GetCategoryBreakdown(type, count, DateTime.MinValue, DateTime.MaxValue);
            }

            return Ok(analyticsCategories);
        }

        [HttpGet("aggregates")]
        public async Task<IActionResult> GetPeriodicAggregates([FromQuery] string type, [FromQuery] string period, [FromQuery] int count)
        {
            return Ok();
        }
    }
}