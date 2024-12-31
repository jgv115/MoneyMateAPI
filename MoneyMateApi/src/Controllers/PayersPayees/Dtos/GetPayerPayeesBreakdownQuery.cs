using System;
using MoneyMateApi.Constants;

namespace MoneyMateApi.Controllers.PayersPayees.Dtos
{
    public record GetPayerPayeesBreakdownQuery
    {
        public TransactionType Type { get; init; }
        public DateTime? Start { get; init; }
        public DateTime? End { get; init; }
    }
}