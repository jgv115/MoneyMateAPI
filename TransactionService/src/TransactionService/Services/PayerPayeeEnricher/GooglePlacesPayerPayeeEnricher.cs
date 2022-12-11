using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TransactionService.Services.PayerPayeeEnricher.Models;
using TransactionService.Services.PayerPayeeEnricher.Options;

namespace TransactionService.Services.PayerPayeeEnricher
{
    public class GooglePlacesPayerPayeeEnricher : IPayerPayeeEnricher
    {
        private readonly HttpClient _httpClient;
        private readonly GooglePlaceApiOptions _placeApiOptions;

        public GooglePlacesPayerPayeeEnricher(HttpClient httpClient, IOptions<GooglePlaceApiOptions> placeApiOptions)
        {
            _httpClient = httpClient;
            _placeApiOptions = placeApiOptions.Value;
        }

        private async Task<GooglePlaceDetailsResponse> _getGooglePlaceDetails(string placeId, string fields)
        {
            var queryParameters = new Dictionary<string, string>
            {
                {"key", _placeApiOptions.ApiKey},
                {"place_id", placeId},
                {"fields", fields}
            };
            var dictFormUrlEncoded = new FormUrlEncodedContent(queryParameters);
            var queryString = await dictFormUrlEncoded.ReadAsStringAsync();
            var url = new Uri(new Uri(_placeApiOptions.GooglePlaceApiBaseUri, UriKind.Absolute),
                new Uri($"maps/api/place/details/json?{queryString}", UriKind.Relative));

            var response = await _httpClient.GetAsync(url);
            return await response.Content.ReadFromJsonAsync<GooglePlaceDetailsResponse>();
        }

        private async Task<string> _refreshPlaceId(string placeId)
        {
            var response = await _getGooglePlaceDetails(placeId, "place_id");

            return response.Result.PlaceId;
        }

        public async Task<ExtraPayerPayeeDetails> GetExtraPayerPayeeDetails(string identifier)
        {
            var placeDetailsResponse = await _getGooglePlaceDetails(identifier, "formatted_address");

            if (placeDetailsResponse.Status == "NOT_FOUND")
            {
                var newPlaceId = await _refreshPlaceId(identifier);
                placeDetailsResponse = await _getGooglePlaceDetails(newPlaceId, "formatted_address");
            }
            
            return new ExtraPayerPayeeDetails
            {
                Address = placeDetailsResponse.Result.FormattedAddress
            };
        }
    }
}