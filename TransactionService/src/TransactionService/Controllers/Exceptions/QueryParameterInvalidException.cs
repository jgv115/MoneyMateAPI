using System;

namespace TransactionService.Controllers.Exceptions
{
    public class QueryParameterInvalidException : Exception
    {
        public QueryParameterInvalidException(string message) : base(message)
        {
        }
    }
}