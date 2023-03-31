using Amazon.DynamoDBv2.DataModel;

namespace TransactionService.Domain.Models
{
    [DynamoDBTable("MoneyMate_TransactionDB_dev")]
    public record PayerPayee
    {
        public string PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
        public string ExternalId { get; set; }
    }
}