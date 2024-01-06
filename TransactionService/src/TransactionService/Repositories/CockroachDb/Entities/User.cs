using System;

namespace TransactionService.Repositories.CockroachDb.Entities
{
    public record User
    {
        public Guid Id { get; init; }
        public string UserIdentifier { get; init; }
    }
}