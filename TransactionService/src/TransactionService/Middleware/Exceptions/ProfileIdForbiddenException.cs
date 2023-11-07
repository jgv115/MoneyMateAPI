using System;

namespace TransactionService.Middleware.Exceptions;

public class ProfileIdForbiddenException : Exception
{
    public ProfileIdForbiddenException(string message) : base(message)
    {
    }
}