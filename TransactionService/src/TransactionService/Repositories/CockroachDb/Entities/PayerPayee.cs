using System;

namespace TransactionService.Repositories.CockroachDb.Entities
{
    public class PayerPayee
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Name { get; init; }
        public PayerPayeeType PayerPayeeType { get; set; }
        public PayerPayeeExternalLinkType PayerPayeeExternalLinkType { get; set; }
        public string ExternalLinkId { get; init; }
    }
}