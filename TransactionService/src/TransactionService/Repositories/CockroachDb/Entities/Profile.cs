using System;

namespace TransactionService.Repositories.CockroachDb.Entities
{
    public record Profile
    {
        public Guid Id { get; init; }
        public string DisplayName { get; init; }
    }
}