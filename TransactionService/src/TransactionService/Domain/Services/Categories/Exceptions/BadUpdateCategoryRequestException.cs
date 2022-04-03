using System;

namespace TransactionService.Domain.Services.Categories.Exceptions
{
    public class BadUpdateCategoryRequestException : Exception
    {
        public BadUpdateCategoryRequestException(string message) : base(message)
        {

        }
    }
}