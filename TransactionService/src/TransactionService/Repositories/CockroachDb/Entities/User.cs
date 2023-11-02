using System;

namespace TransactionService.Repositories.CockroachDb.Entities
{
    public class User
    {
        public Guid Id { get; init; }
        public string UserIdentifier { get; init; }
    }
}