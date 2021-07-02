using Amazon.DynamoDBv2.DataModel;

namespace TransactionService.Models
{
    [DynamoDBTable("MoneyMate_TransactionDB_dev")]
    public record Transaction

    {
        [DynamoDBHashKey("UserIdQuery")] public string UserId { get; set; }
        [DynamoDBRangeKey("Subquery")] public string TransactionId { get; set; }
        public string TransactionTimestamp { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; init; }
        public string Category { get; init; }
        public string SubCategory { get; init; }
        public string Note { get; init; }
    }
}