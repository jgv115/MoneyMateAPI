namespace MoneyMateApi.Domain.Models
{
    public record Transaction
    {
        public string TransactionId { get; set; }
        // TODO: change this to datetimeoffset
        public string TransactionTimestamp { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; init; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        // TODO: make this uuid
        public string PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
        public string Note { get; init; }
    }
}