namespace TransactionService.ViewModels
{
    public record AnalyticsCategory
    {
        public string CategoryName { get; init; }
        public decimal TotalAmount { get; init; }
    }
}