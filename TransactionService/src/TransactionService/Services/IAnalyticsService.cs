using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.ViewModels;

namespace TransactionService.Services
{
    public interface IAnalyticsService
    {
        public Task<IEnumerable<AnalyticsCategory>> GetCategoryBreakdown(string type, int? count, DateTime start,
            DateTime end);

        public Task<IEnumerable<AnalyticsSubcategory>> GetSubcategoriesBreakdown(string categoryName, int? count,
            DateTime start, DateTime end);
    }
}