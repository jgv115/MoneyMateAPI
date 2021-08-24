using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace TransactionService.Models
{
    [DynamoDBTable("MoneyMate_TransactionDB_dev")]
    public record PayerPayee
    {
        [DynamoDBHashKey("UserIdQuery")] public string UserId { get; set; }
        [DynamoDBRangeKey("Subquery")] public string Name { get; set; }
        public string ExternalId { get; set; }
    }
}