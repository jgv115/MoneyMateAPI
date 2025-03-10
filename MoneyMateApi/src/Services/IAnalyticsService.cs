using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Analytics.ViewModels;
using MoneyMateApi.Helpers.TimePeriodHelpers;

namespace MoneyMateApi.Services
{
    public interface IAnalyticsService
    {
        public Task<IEnumerable<AnalyticsCategory>> GetCategoriesBreakdown(TransactionType type, int? count, DateTime start,
            DateTime end);

        public Task<IEnumerable<AnalyticsCategory>> GetCategoriesBreakdown(TransactionType type, int? count, TimePeriod timePeriod);

        public Task<IEnumerable<AnalyticsSubcategory>> GetSubcategoriesBreakdown(string categoryName, int? count,
            DateTime start, DateTime end);

        public Task<IEnumerable<AnalyticsPayerPayee>> GetPayerPayeeBreakdown(TransactionType type, DateTime start,
            DateTime end);
    }
}