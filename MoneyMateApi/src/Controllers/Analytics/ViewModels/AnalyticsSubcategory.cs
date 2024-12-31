namespace MoneyMateApi.Controllers.Analytics.ViewModels
{
    public record AnalyticsSubcategory
    {
        public string SubcategoryName { get; init; }
        public string BelongsToCategory { get; init; }
        public decimal TotalAmount { get; init; }
    }
}