using System;

namespace TransactionService.Repositories.CockroachDb.Entities
{
    public class TransactionType
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
    }
}