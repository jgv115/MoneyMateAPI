using System.Text.Json.Serialization;

namespace MoneyMateApi.Services.PayerPayeeEnricher.Models
{
    public record GooglePlaceDetailsResponse
    {
        [JsonPropertyName("result")] public GooglePlaceDetailsResult Result { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; }
    }

    public record GooglePlaceDetailsResult
    {
        [JsonPropertyName("formattedAddress")]
        public string FormattedAddress { get; set; }

        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; }
    }
}