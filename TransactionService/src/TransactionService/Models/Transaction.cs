using Amazon.DynamoDBv2.DataModel;

namespace TransactionService.Models
{
    [DynamoDBTable("MoneyMate_TransactionDB_test")]
    public class Transaction

    {
        [DynamoDBHashKey("UserId-TransactionType")]
        public string UserIdTransactionType { get; init; }

        [DynamoDBRangeKey("Date")] public string Date { get; init; }
        public string TransactionId { get; init; }
        public decimal Amount { get; init; }
        public string Category { get; init; }
        public string SubCategory { get; init; }
    }
}