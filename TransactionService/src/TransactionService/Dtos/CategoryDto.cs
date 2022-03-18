using System.Collections.Generic;
using TransactionService.Constants;

namespace TransactionService.Dtos
{
    public record CategoryDto
    {
        public string CategoryName { get; set; }
        public TransactionType TransactionType { get; set; }
        public List<string> Subcategories { get; set; }
    }
}