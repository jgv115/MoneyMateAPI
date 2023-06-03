using System;

namespace TransactionService.Repositories.Exceptions
{
    public class RepositoryItemDoesNotExist : Exception
    {
        public RepositoryItemDoesNotExist(string message) : base(message)
        {
        }
    }
}