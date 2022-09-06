using Amazon.DynamoDBv2.DataModel;

namespace TransactionService.Domain.Models
{
    [DynamoDBTable("MoneyMate_TransactionDB_dev")]
    public record Transaction
    {
        [DynamoDBHashKey("UserIdQuery")] public string UserId { get; set; }
        [DynamoDBRangeKey("Subquery")] public string TransactionId { get; set; }

        [DynamoDBLocalSecondaryIndexRangeKey("TransactionTimestamp")]
        public string TransactionTimestamp { get; set; }

        public string TransactionType { get; set; }
        public decimal Amount { get; init; }
        public string Category { get; set; }
        [DynamoDBProperty("SubCategory")] public string Subcategory { get; set; }
        public string PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
        public string Note { get; init; }
    }
}