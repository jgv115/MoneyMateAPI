using System.Text.Json.Serialization;

namespace TransactionService.Services.PayerPayeeEnricher.Models
{
    public record GooglePlaceDetailsResponse
    {
        [JsonPropertyName("result")] public GooglePlaceDetailsResult Result { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; }
    }

    public record GooglePlaceDetailsResult
    {
        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; }
    }
}