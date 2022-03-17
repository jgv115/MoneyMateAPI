using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using TransactionService.Constants;

namespace TransactionService.Domain.Models
{
    [DynamoDBTable("MoneyMate_TransactionDB_dev")]
    public record Category
    {
        [DynamoDBHashKey("UserIdQuery")] public string UserId { get; set; }
        [DynamoDBRangeKey("Subquery")] public string CategoryName { get; set; }
        public TransactionType TransactionType { get; set; }
        public List<string> Subcategories { get; set; }
    }
}