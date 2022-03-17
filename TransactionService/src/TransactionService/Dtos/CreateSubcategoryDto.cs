namespace TransactionService.Dtos
{
    public record CreateSubcategoryDto
    {
        public string CategoryName { get; set; }
        public string CategoryType { get; set; }
        public string SubcategoryName { get; set; }
    }
}