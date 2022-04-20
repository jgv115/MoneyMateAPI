using System;
using TransactionService.Constants;

namespace TransactionService.Dtos
{
    public record GetPayerPayeesBreakdownQuery
    {
        public TransactionType Type { get; init; }
        public DateTime? Start { get; init; }
        public DateTime? End { get; init; }
    }
}