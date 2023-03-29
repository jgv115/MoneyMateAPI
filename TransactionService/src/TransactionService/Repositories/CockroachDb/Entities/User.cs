using System;
using System.Collections.Generic;

namespace TransactionService.Repositories.CockroachDb.Entities
{
    public class User
    {
        public Guid Id { get; init; }
        public string UserIdentifier { get; init; }
        public List<Category> Categories { get; init; }
    }
}