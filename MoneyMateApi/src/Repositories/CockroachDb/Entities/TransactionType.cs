using System;

namespace MoneyMateApi.Repositories.CockroachDb.Entities
{
    public class TransactionType
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
    }
}