using System;

namespace TransactionService.Repositories.CockroachDb.Entities
{
    public class PayerPayeeExternalLinkType
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
    }
}