using System;
using TransactionService.Constants;

namespace TransactionService.Controllers.Categories.Dtos
{
    public record GetCategoriesBreakdownQuery
    {
        public TransactionType? Type { get; init; }
        public int? Count { get; init; }
        public DateTime? Start { get; init; }
        public DateTime? End { get; init; }
        public string Frequency { get; set; }
        public int? Periods { get; init; }
    }
}