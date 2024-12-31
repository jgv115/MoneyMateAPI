using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MoneyMateApi.Constants;

namespace MoneyMateApi.Controllers.Transactions.Dtos
{
    public record GetTransactionsQuery
    {
        public TransactionType? Type { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public List<string> Categories { get; set; } = new();
        [FromQuery(Name = "subcategory")] public List<string> Subcategories { get; set; } = new();
        [FromQuery(Name = "payerPayeeId")] public List<string> PayerPayeeIds { get; set; } = new();

        public virtual bool Equals(GetTransactionsQuery other)
        {
            return other != null && Type.Equals(other.Type) && Start.Equals(other.Start) && End.Equals(other.End) &&
                   Categories.SequenceEqual(other.Categories) && Subcategories.SequenceEqual(other.Subcategories) &&
                   PayerPayeeIds.SequenceEqual(other.PayerPayeeIds);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Start, End, Categories, Subcategories, PayerPayeeIds);
        }
    }
}