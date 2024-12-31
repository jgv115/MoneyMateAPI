using System;

namespace MoneyMateApi.Controllers.Exceptions
{
    public class QueryParameterInvalidException : Exception
    {
        public QueryParameterInvalidException(string message) : base(message)
        {
        }
    }
}