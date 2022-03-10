using System;

namespace TransactionService.Repositories.Exceptions
{
    public class RepositoryItemExistsException : Exception
    {
        public RepositoryItemExistsException(string message) : base(message)
        {
        }
    }
}