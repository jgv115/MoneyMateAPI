using System;

namespace MoneyMateApi.Repositories.CockroachDb.Entities
{
    public class PayerPayeeType
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
    }
}