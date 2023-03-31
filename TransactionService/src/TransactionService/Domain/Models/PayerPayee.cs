namespace TransactionService.Domain.Models
{
    public record PayerPayee
    {
        public string PayerPayeeId { get; set; }
        public string PayerPayeeName { get; set; }
        public string ExternalId { get; set; }
    }
}