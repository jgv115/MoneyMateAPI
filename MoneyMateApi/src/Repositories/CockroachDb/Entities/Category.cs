using System;
using System.Collections.Generic;

namespace MoneyMateApi.Repositories.CockroachDb.Entities
{
    public record Category
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Name { get; init; }
        public string TransactionType { get; set; }
        public List<Subcategory> Subcategories { get; init; } = new();
    }
}