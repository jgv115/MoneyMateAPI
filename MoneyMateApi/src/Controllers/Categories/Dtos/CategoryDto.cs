using System.Collections.Generic;
using MoneyMateApi.Constants;

namespace MoneyMateApi.Controllers.Categories.Dtos
{
    public record CategoryDto
    {
        public string CategoryName { get; set; }
        public TransactionType TransactionType { get; set; }
        public List<string> Subcategories { get; set; }
    }
}