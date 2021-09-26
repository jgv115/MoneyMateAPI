using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Domain.Services;
using TransactionService.ViewModels;

namespace TransactionService.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ITransactionHelperService _transactionService;

        public AnalyticsService(ITransactionHelperService transactionService)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        }

        public async Task<IEnumerable<AnalyticsCategory>> GetCategoryBreakdown(string type, int? count, DateTime start,
            DateTime end)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync(start, end, type);
            var orderedCategories = transactions.GroupBy(transaction => transaction.Category)
                .Select(grouping => new AnalyticsCategory
                {
                    CategoryName = grouping.Key, TotalAmount = grouping.Sum(transaction => transaction.Amount)
                })
                .OrderByDescending(categoryAmounts => categoryAmounts.TotalAmount);

            if (count.HasValue) return orderedCategories.Take(count.Value);
            else return orderedCategories;
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