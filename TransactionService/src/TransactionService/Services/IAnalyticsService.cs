using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.ViewModels;

namespace TransactionService.Services
{
    public interface IAnalyticsService
    {
        public Task<IEnumerable<AnalyticsCategory>> GetCategoriesBreakdown(string type, int? count, DateTime start,
            DateTime end);

        public Task<IEnumerable<AnalyticsCategory>> GetCategoriesBreakdown(string type, int? count, TimePeriod timePeriod);

        public Task<IEnumerable<AnalyticsSubcategory>> GetSubcategoriesBreakdown(string categoryName, int? count,
            DateTime start, DateTime end);
    }
}