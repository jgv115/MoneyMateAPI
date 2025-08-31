using System;

namespace MoneyMateApi.Domain.Categories.Exceptions
{
    public class UpdateCategoryOperationException : Exception
    {
        public UpdateCategoryOperationException(string message) : base(message)
        {
        }
    }
}