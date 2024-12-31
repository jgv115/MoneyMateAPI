namespace MoneyMateApi.Controllers.Transactions.Dtos
{
    public class StoreTransactionDto
    {
        public string TransactionTimestamp { get; init; }
        public string TransactionType { get; init; }
        public decimal Amount { get; init; }
        public string Category { get; init; }
        public string Subcategory { get; init; }
        public string PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
        public string Note { get; init; }
    }
}