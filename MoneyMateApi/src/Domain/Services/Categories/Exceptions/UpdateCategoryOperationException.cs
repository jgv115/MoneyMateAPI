using System;

namespace MoneyMateApi.Domain.Services.Categories.Exceptions
{
    public class UpdateCategoryOperationException : Exception
    {
        public UpdateCategoryOperationException(string message) : base(message)
        {
        }
    }
}