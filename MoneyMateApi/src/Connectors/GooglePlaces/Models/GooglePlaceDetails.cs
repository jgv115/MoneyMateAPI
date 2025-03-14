namespace MoneyMateApi.Connectors.GooglePlaces.Models;

public record GooglePlaceDetails
{
    public required string Id { get; set; }

    public string FormattedAddress { get; set; } = "";
}