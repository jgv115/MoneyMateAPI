using System;

namespace MoneyMateApi.Repositories.Exceptions
{
    public class RepositoryItemDoesNotExist : Exception
    {
        public RepositoryItemDoesNotExist(string message) : base(message)
        {
        }
    }
}