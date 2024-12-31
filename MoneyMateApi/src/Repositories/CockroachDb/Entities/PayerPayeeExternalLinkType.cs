using System;

namespace MoneyMateApi.Repositories.CockroachDb.Entities
{
    public class PayerPayeeExternalLinkType
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
    }
}