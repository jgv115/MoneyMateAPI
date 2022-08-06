using System;

namespace TransactionService.ViewModels
{
    public record AnalyticsPayerPayee
    {
        public string PayerPayeeId { get; set; }
        public string PayerPayeeName { get; init; }
        public decimal TotalAmount { get; init; }
    }
}