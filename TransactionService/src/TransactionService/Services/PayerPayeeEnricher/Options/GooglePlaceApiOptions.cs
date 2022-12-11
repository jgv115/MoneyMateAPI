namespace TransactionService.Services.PayerPayeeEnricher.Options
{
    public record GooglePlaceApiOptions
    {
        public string GooglePlaceApiBaseUri { get; set; }
        public string ApiKey { get; set; }
    }
}