using System;

namespace TransactionService.ViewModels
{
    public record PayerPayeeViewModel
    {
        public Guid PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
        public string ExternalId { get; set; }
    }
}