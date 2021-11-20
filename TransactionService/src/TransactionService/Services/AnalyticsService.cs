using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Domain.Services;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.ViewModels;

namespace TransactionService.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ITransactionHelperService _transactionService;
        private readonly ITimePeriodHelper _timePeriodHelper;

        public AnalyticsService(ITransactionHelperService transactionService, ITimePeriodHelper timePeriodHelper)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _timePeriodHelper = timePeriodHelper ?? throw new ArgumentNullException(nameof(timePeriodHelper));
        }

        public async Task<IEnumerable<AnalyticsCategory>> GetCategoryBreakdown(string type, int? count, DateTime start,
            DateTime end)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync(start, end, type);
            var orderedCategories = transactions.GroupBy(transaction => transaction.Category)
                .Select(grouping => new AnalyticsCategory
                {
                    CategoryName = grouping.Key,
                    TotalAmount = grouping.Sum(transaction => transaction.Amount)
                })
                .OrderByDescending(categoryAmounts => categoryAmounts.TotalAmount);

            return count.HasValue ? orderedCategories.Take(count.Value) : orderedCategories;
        }

        public async Task<IEnumerable<AnalyticsCategory>> GetCategoryBreakdown(string type, int? count, TimePeriod timePeriod)
        {
            var dateRange = _timePeriodHelper.ResolveDateRange(timePeriod);
            return await GetCategoryBreakdown(type, count, dateRange.Start, dateRange.End);
        }

        public async Task<IEnumerable<AnalyticsSubcategory>> GetSubcategoriesBreakdown(string categoryName, int? count,
            DateTime start, DateTime end)
        {
            var transactions = await _transactionService.GetAllTransactionsByCategoryAsync(categoryName, start, end);
            var orderedSubcategories = transactions.GroupBy(transaction => transaction.SubCategory)
                .Select(grouping => new AnalyticsSubcategory
                {
                    SubcategoryName = grouping.Key,
                    BelongsToCategory = categoryName,
                    TotalAmount = grouping.Sum(transaction => transaction.Amount)
                })
                .OrderByDescending(subcategories => subcategories.TotalAmount);

            if (count.HasValue) return orderedSubcategories.Take(count.Value);
            else return orderedSubcategories;
        }
    }
}