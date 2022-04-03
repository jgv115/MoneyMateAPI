using System;

namespace TransactionService.Domain.Services.Categories.Exceptions
{
    public class CategoryDoesNotExistException : Exception
    {
        public CategoryDoesNotExistException(string message) : base(message)
        {

        }
    }
}