using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Constants;
using TransactionService.Controllers.Analytics.ViewModels;
using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Domain.Services;
using TransactionService.Domain.Services.Transactions;
using TransactionService.Helpers.TimePeriodHelpers;

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

        public async Task<IEnumerable<AnalyticsCategory>> GetCategoriesBreakdown(TransactionType type, int? count,
            DateTime start,
            DateTime end)
        {
            var transactions = await _transactionService.GetTransactionsAsync(new GetTransactionsQuery
            {
                Start = start,
                End = end,
                Type = type,
            });
            var orderedCategories = transactions.GroupBy(transaction => transaction.Category)
                .Select(grouping => new AnalyticsCategory
                {
                    CategoryName = grouping.Key,
                    TotalAmount = grouping.Sum(transaction => transaction.Amount)
                })
                .OrderByDescending(categoryAmounts => categoryAmounts.TotalAmount);

            return count.HasValue ? orderedCategories.Take(count.Value) : orderedCategories;
        }

        public async Task<IEnumerable<AnalyticsCategory>> GetCategoriesBreakdown(TransactionType type, int? count,
            TimePeriod timePeriod)
        {
            var dateRange = _timePeriodHelper.ResolveDateRange(timePeriod);
            return await GetCategoriesBreakdown(type, count, dateRange.Start, dateRange.End);
        }

        public async Task<IEnumerable<AnalyticsSubcategory>> GetSubcategoriesBreakdown(string categoryName, int? count,
            DateTime start, DateTime end)
        {
            var transactions = await _transactionService.GetTransactionsAsync(new GetTransactionsQuery
            {
                Categories = new List<string> { categoryName },
                Start = start,
                End = end,
            });

            var orderedSubcategories = transactions.GroupBy(transaction => transaction.Subcategory)
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

        public async Task<IEnumerable<AnalyticsPayerPayee>> GetPayerPayeeBreakdown(TransactionType transactionType,
            DateTime start, DateTime end)
        {
            var transactions = await _transactionService.GetTransactionsAsync(new GetTransactionsQuery
            {
                Type = transactionType,
                Start = start,
                End = end
            });

            var orderedPayerPayees = transactions
                .GroupBy(transaction => new { transaction?.PayerPayeeId, transaction?.PayerPayeeName })
                .Select(grouping => new AnalyticsPayerPayee
                {
                    PayerPayeeId = grouping.Key.PayerPayeeId ?? "",
                    PayerPayeeName = string.IsNullOrEmpty(grouping.Key.PayerPayeeId)
                        ? "Unspecified"
                        : grouping.Key.PayerPayeeName,
                    TotalAmount = grouping.Sum(transaction => transaction.Amount)
                })
                .OrderByDescending(analyticsPayerPayee => analyticsPayerPayee.TotalAmount);

            return orderedPayerPayees;
        }
    }
}