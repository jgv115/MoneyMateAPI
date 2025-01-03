using System;
using System.Collections.Generic;
using System.Linq;
using MoneyMateApi.Domain.Models;

namespace MoneyMateApi.Controllers.Transactions.Dtos;

public record TransactionOutputDto
{
    public required string TransactionId { get; set; }
    public required string TransactionTimestamp { get; set; }
    public required string TransactionType { get; set; }
    public required decimal Amount { get; init; }
    public required string Category { get; set; }
    public required string Subcategory { get; set; }
    public string? PayerPayeeId { get; set; }
    public string? PayerPayeeName { get; set; }
    public string? Note { get; init; }
    public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();

    public virtual bool Equals(TransactionOutputDto? other)
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
               Tags.SequenceEqual(other.Tags);
    }

    public override int GetHashCode()
    {
        var tagsHashCode = Tags.Aggregate(0, HashCode.Combine);
        var hash1 = HashCode.Combine(TransactionId, TransactionTimestamp, TransactionType, Amount);
        var hash2 = HashCode.Combine(Category, Subcategory, PayerPayeeId, PayerPayeeName, Note);
        return HashCode.Combine(hash1, hash2, tagsHashCode);
    }
}