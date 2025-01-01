using System;

namespace MoneyMateApi.Repositories.CockroachDb.Entities;

public record Tag
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required Guid ProfileId { get; set; }
    public required DateTime CreatedAt { get; set; }
}