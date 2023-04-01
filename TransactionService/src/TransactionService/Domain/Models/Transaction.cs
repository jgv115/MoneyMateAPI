namespace TransactionService.Domain.Models
{
    public record Transaction
    {
        public string TransactionId { get; set; }
        public string TransactionTimestamp { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; init; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public string PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
        public string Note { get; init; }
    }
}