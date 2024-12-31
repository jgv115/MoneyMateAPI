using System;

namespace MoneyMateApi.Repositories.Exceptions
{
    public class RepositoryItemExistsException : Exception
    {
        public RepositoryItemExistsException(string message) : base(message)
        {
        }
    }
}