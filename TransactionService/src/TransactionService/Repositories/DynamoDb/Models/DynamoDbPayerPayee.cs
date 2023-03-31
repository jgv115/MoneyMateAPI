using Amazon.DynamoDBv2.DataModel;

namespace TransactionService.Repositories.DynamoDb.Models
{
    [DynamoDBTable("MoneyMate_TransactionDB_dev")]
    public record DynamoDbPayerPayee
    {
        [DynamoDBHashKey("UserIdQuery")] public string UserId { get; set; }
        [DynamoDBRangeKey("Subquery")] public string PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
        public string ExternalId { get; set; }
    }
}