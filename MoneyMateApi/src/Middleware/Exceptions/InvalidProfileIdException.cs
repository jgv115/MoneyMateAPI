using System;

namespace MoneyMateApi.Middleware.Exceptions
{
    public class InvalidProfileIdException : Exception
    {
        public InvalidProfileIdException(string message) : base(message)
        {
        }
    }
}