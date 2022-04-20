using System;

namespace TransactionService.ViewModels
{
    public record AnalyticsPayerPayee
    {
        public Guid PayerPayeeId { get; set; }
        public string PayerPayeeName { get; init; }
        public decimal TotalAmount { get; init; }
    }
}