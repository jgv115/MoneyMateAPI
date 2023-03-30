using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.DataModel;
using TransactionService.Constants;

namespace TransactionService.Repositories.DynamoDb.Models
{
    [DynamoDBTable("MoneyMate_TransactionDB_dev")]
    public record Category
    {
        [DynamoDBHashKey("UserIdQuery")] public string UserId { get; set; }
        [DynamoDBRangeKey("Subquery")] public string CategoryName { get; set; }
        public TransactionType TransactionType { get; set; }
        public List<string> Subcategories { get; set; } = new List<string>();

        public virtual bool Equals(Category? other)
        {
            return other != null && other.UserId == UserId && other.CategoryName == CategoryName &&
                   other.TransactionType == TransactionType && other.Subcategories.SequenceEqual(Subcategories);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, CategoryName, (int)TransactionType, Subcategories);
        }
    }
}