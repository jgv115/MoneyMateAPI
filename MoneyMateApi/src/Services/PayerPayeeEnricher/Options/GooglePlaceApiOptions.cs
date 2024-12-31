namespace MoneyMateApi.Services.PayerPayeeEnricher.Options
{
    public record GooglePlaceApiOptions
    {
        public string PlaceDetailsBaseUri { get; set; }
        public string ApiKey { get; set; }
    }
}