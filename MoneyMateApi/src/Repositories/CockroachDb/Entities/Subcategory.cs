using System;

namespace MoneyMateApi.Repositories.CockroachDb.Entities
{
    public class Subcategory
    {
        public Guid Id { get; init; }
        public Guid CategoryId { get; init; }
        public string Name { get; init; }

        public Category Category { get; init; }
    }
}