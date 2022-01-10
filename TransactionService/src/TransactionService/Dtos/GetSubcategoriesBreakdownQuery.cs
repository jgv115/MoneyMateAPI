#nullable enable
using System;

namespace TransactionService.Dtos
{
    public record GetSubcategoriesBreakdownQuery
    {
        public string? Category { get; init; }
        public int? Count { get; init; }
        public DateTime? Start { get; init; }
        public DateTime? End { get; init; }
    }
}