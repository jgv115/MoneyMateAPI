using System;
using System.Collections.Generic;

namespace TransactionService.Repositories.CockroachDb.Entities
{
    public record Category
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Name { get; init; }
        public string TransactionType { get; init; }
        public List<Subcategory> Subcategories { get; init; } = new();
    }
}