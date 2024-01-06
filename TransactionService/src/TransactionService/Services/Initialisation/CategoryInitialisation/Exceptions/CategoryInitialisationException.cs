using System;

namespace TransactionService.Services.Initialisation.CategoryInitialisation.Exceptions;

public class CategoryInitialisationException : Exception
{
    public CategoryInitialisationException(string message) : base(message)
    {
    }

    public CategoryInitialisationException(string message, Exception ex) : base(message, ex)
    {
    }
}