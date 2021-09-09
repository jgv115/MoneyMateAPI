using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace TransactionService.Domain.Models
{
    [DynamoDBTable("MoneyMate_TransactionDB_dev")]
    public record Category
    {
        [DynamoDBHashKey("UserIdQuery")] public string UserId { get; set; }
        [DynamoDBRangeKey("Subquery")] public string CategoryName { get; set; }
        public List<string> SubCategories { get; set; }
    }
}