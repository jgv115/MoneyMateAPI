using System;

namespace TransactionService.Domain.Models
{
    public record Profile
    {
        public Guid Id { get; init; }
        public string DisplayName { get; init; }
    }
}