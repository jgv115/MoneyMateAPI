using System;

namespace MoneyMateApi.Domain.Models
{
    public record Profile
    {
        public Guid Id { get; init; }
        public string DisplayName { get; init; }
    }
}