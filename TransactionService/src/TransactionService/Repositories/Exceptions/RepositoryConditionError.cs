using System;

namespace TransactionService.Repositories.Exceptions
{
    public class RepositoryConditionError : Exception
    {
        public RepositoryConditionError(string message) : base(message)
        {
        }
    }
}