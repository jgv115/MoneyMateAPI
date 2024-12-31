using System;

namespace MoneyMateApi.Controllers.PayersPayees.ViewModels
{
    public record PayerPayeeViewModel
    {
        public Guid PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
        public string ExternalId { get; set; } = "";
        public string Address { get; set; }
    }
}