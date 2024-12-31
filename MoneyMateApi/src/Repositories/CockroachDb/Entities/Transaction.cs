using System;

namespace MoneyMateApi.Repositories.CockroachDb.Entities
{
    public class Transaction
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public DateTime TransactionTimestamp { get; set; }
        public decimal Amount { get; init; }
        public string Note { get; init; }

        public string TransactionType { get; set; }
        
        public string Category { get; set; }
        public string Subcategory { get; set; }
        
        public Guid PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
    }
}