using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Constants;

namespace TransactionService.Dtos
{
    public record GetTransactionsQuery
    {
        public TransactionType? Type { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public List<string> Categories { get; set; } = new();
        [FromQuery(Name = "subcategory")] public List<string> Subcategories { get; set; } = new();
        [FromQuery(Name = "payerPayeeId")] public List<string> PayerPayeeIds { get; set; } = new();
    }
}