namespace TransactionService.Dtos
{
    public class PutTransactionDto
    {
        public string TransactionId { get; init; }
        public string TransactionTimestamp { get; init; }
        public string TransactionType { get; init; }
        public decimal Amount { get; init; }
        public string Category { get; init; }
        public string SubCategory { get; init; }
    }
}