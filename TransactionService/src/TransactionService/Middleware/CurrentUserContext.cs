using System;

namespace TransactionService.Middleware
{
    public class CurrentUserContext
    {
        public string UserId { get; set; }
        public Guid ProfileId { get; set; }
    }
}