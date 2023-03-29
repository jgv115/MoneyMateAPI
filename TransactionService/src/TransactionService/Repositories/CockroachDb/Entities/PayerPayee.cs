using System;

namespace TransactionService.Repositories.CockroachDb.Entities
{
    public class PayerPayee
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Name { get; init; }
        public PayerPayeeType PayerPayeeType { get; init; }
        public PayerPayeeExternalLinkType PayerPayeeExternalLinkType { get; init; }
        public string ExternalLinkId { get; init; }
    }
}