namespace TransactionService.Dtos
{
    public class PutTransactionDto
    {
        public string TransactionTimestamp { get; init; }
        public string TransactionType { get; init; }
        public decimal Amount { get; init; }
        public string Category { get; init; }
        public string SubCategory { get; init; }
        public string PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
        public string Note { get; init; }
    }
}