using System;

namespace MoneyMateApi.Domain.Profiles
{
    public record Profile
    {
        public Guid Id { get; init; }
        public string DisplayName { get; init; }
    }
}