using System;

namespace MoneyMateApi.Repositories.CockroachDb.Entities
{
    public record User
    {
        public Guid Id { get; init; }
        public string UserIdentifier { get; init; }
    }
}