using System;
using System.Collections.Generic;
using System.Linq;

namespace MoneyMateApi.Domain.Transactions
{
    public record Transaction
    {
        public string TransactionId { get; set; }

        // TODO: change this to datetimeoffset
        public string TransactionTimestamp { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; init; }
        public string Category { get; set; }

        public string Subcategory { get; set; }

        // TODO: make this uuid
        public string PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
        public string Note { get; init; }
        public IEnumerable<Guid> TagIds { get; set; } = new List<Guid>();

        public virtual bool Equals(Transaction other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return TransactionId == other.TransactionId &&
                   // TODO: modify this if we change type of transaction timestamp
                   DateTime.Parse(TransactionTimestamp) == DateTime.Parse(other.TransactionTimestamp) &&
                   TransactionType == other.TransactionType &&
                   Amount == other.Amount &&
                   Category == other.Category &&
                   Subcategory == other.Subcategory &&
                   PayerPayeeId == other.PayerPayeeId &&
                   PayerPayeeName == other.PayerPayeeName &&
                   Note == other.Note &&
                   TagIds.SequenceEqual(other.TagIds);
        }

        public override int GetHashCode()
        {
            var tagsHashCode = TagIds.Aggregate(0, HashCode.Combine);
            var hash1 = HashCode.Combine(TransactionId, TransactionTimestamp, TransactionType, Amount);
            var hash2 = HashCode.Combine(Category, Subcategory, PayerPayeeId, PayerPayeeName, Note);
            return HashCode.Combine(hash1, hash2, tagsHashCode);
        }
    }
}