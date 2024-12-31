using System;

namespace MoneyMateApi.Middleware.Exceptions;

public class ProfileIdForbiddenException : Exception
{
    public ProfileIdForbiddenException(string message) : base(message)
    {
    }
}