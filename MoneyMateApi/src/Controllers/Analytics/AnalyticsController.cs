using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyMateApi.Controllers.Analytics.ViewModels;
using MoneyMateApi.Controllers.Categories.Dtos;
using MoneyMateApi.Controllers.PayersPayees.Dtos;
using MoneyMateApi.Helpers.TimePeriodHelpers;
using MoneyMateApi.Services;

namespace MoneyMateApi.Controllers.Analytics
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
        public async Task<IActionResult> GetCategoriesBreakdown([FromQuery] GetCategoriesBreakdownQuery queryParams)
        {
            var type = queryParams.Type.Value;
            var count = queryParams.Count;
            var start = queryParams.Start;
            var end = queryParams.End;
            var frequency = queryParams.Frequency;
            var periods = queryParams.Periods;

            IEnumerable<AnalyticsCategory> analyticsCategories;
            if (start.HasValue && end.HasValue)
            {
                analyticsCategories =
                    await _analyticsService.GetCategoriesBreakdown(type, count, start.Value, end.Value);
            }
            else if (!string.IsNullOrEmpty(frequency) && periods.HasValue)
            {
                analyticsCategories =
                    await _analyticsService.GetCategoriesBreakdown(type, count,
                        new TimePeriod(frequency, periods.Value));
            }
            else
            {
                analyticsCategories =
                    await _analyticsService.GetCategoriesBreakdown(type, count, DateTime.MinValue, DateTime.MaxValue);
            }

            return Ok(analyticsCategories);
        }

        [HttpGet("subcategories")]
        public async Task<IActionResult> GetSubcategoriesBreakdown(
            [FromQuery] GetSubcategoriesBreakdownQuery queryParams)
        {
            var category = queryParams.Category;
            var count = queryParams.Count;
            var start = queryParams.Start;
            var end = queryParams.End;


            IEnumerable<AnalyticsSubcategory> analyticsCategories;
            if (start.HasValue && end.HasValue)
            {
                analyticsCategories =
                    await _analyticsService.GetSubcategoriesBreakdown(category, count, start.Value, end.Value);
            }
            else
            {
                analyticsCategories =
                    await _analyticsService.GetSubcategoriesBreakdown(category, count, DateTime.MinValue,
                        DateTime.MaxValue);
            }

            return Ok(analyticsCategories);
        }

        [HttpGet("payerPayees")]
        public async Task<IActionResult> GetPayerPayeesBreakdown(
            [FromQuery] GetPayerPayeesBreakdownQuery queryParams)
        {
            var type = queryParams.Type;
            var start = queryParams.Start;
            var end = queryParams.End;


            IEnumerable<AnalyticsPayerPayee> analyticsCategoriesPayees;
            if (start.HasValue && end.HasValue)
            {
                analyticsCategoriesPayees =
                    await _analyticsService.GetPayerPayeeBreakdown(type, start.Value, end.Value);
            }
            else
            {
                analyticsCategoriesPayees =
                    await _analyticsService.GetPayerPayeeBreakdown(type, DateTime.MinValue,
                        DateTime.MaxValue);
            }

            return Ok(analyticsCategoriesPayees);
        }

        [HttpGet("aggregates")]
        public async Task<IActionResult> GetPeriodicAggregates([FromQuery] string type, [FromQuery] string period,
            [FromQuery] int count)
        {
            return Ok();
        }
    }
}