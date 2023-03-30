namespace TransactionService.Controllers.Analytics.ViewModels
{
    public record AnalyticsCategory
    {
        public string CategoryName { get; init; }
        public decimal TotalAmount { get; init; }
    }
}