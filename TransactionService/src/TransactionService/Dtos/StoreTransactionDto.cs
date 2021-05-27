namespace TransactionService.Dtos
{
    public class StoreTransactionDto
    {
        public string TransactionTimestamp { get; init; }
        public string TransactionType { get; init; }
        public decimal Amount { get; init; }
        public string Category { get; init; }
        public string SubCategory { get; init; }
    }
}