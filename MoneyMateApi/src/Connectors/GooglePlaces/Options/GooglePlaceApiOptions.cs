namespace MoneyMateApi.Connectors.GooglePlaces.Options
{
    public record GooglePlaceApiOptions
    {
        public string PlaceDetailsBaseUri { get; set; } = "";
        public string ApiKey { get; set; } = "";
    }
}