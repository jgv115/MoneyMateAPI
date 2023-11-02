using System;

namespace TransactionService.Middleware.Exceptions
{
    public class InvalidProfileIdException : Exception
    {
        public InvalidProfileIdException(string message) : base(message)
        {
        }
    }
}