using System.Text.Json.Serialization;
using MoneyMateApi.Connectors.GooglePlaces.Models;

namespace MoneyMateApi.Connectors.GooglePlaces.Dtos;

public record GooglePlaceDetailsError
{
    [JsonPropertyName("code")] public int Code { get; set; }

    [JsonPropertyName("message")] public required string Message { get; set; }

    [JsonPropertyName("status")] public required string Status { get; set; }
}

public record GooglePlaceDetailsResponse
{
    [JsonPropertyName("formattedAddress")] public string? FormattedAddress { get; set; }

    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("error")] public GooglePlaceDetailsError? Error { get; set; }
}

public static class GooglePlaceDetailsResponseExtensions
{
    public static GooglePlaceDetails ToGooglePlaceDetails(this GooglePlaceDetailsResponse response, string placeId)
    {
        return new GooglePlaceDetails
        {
            Id = placeId,
            FormattedAddress = response.FormattedAddress ?? "",
        };
    }
}