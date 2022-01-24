using System;
using System.Collections.Generic;
using TransactionService.Constants;

namespace TransactionService.Dtos
{
    public record GetTransactionsQuery
    {
        public TransactionType? Type { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public List<string> Categories { get; set; } = new();
        public List<string> SubcategoriesQuery { get; set; } = new();
    }
}