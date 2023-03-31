using System;
using System.Collections.Generic;
using System.Linq;
using TransactionService.Constants;

namespace TransactionService.Domain.Models
{
    public record Category
    {
        public string CategoryName { get; set; }
        public TransactionType TransactionType { get; set; }
        public List<string> Subcategories { get; set; } = new();

        public virtual bool Equals(Category? other)
        {
            return other != null && other.CategoryName == CategoryName &&
                   other.TransactionType == TransactionType && other.Subcategories.SequenceEqual(Subcategories);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CategoryName, (int) TransactionType, Subcategories);
        }
    }
}