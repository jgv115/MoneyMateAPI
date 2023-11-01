using System;

namespace TransactionService.Domain.Models
{
    public class Profile
    {
        public Guid Id { get; init; }
        public string DisplayName { get; init; }
    }
}