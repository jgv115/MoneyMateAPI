using System.Collections.Generic;

namespace TransactionService.Dtos
{
    public record CreateCategoryDto
    {
        public string CategoryName { get; set; }
        public string CategoryType { get; set; }
        public List<string> SubCategories { get; set; }
    }
}