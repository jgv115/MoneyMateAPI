using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TransactionService.Constants;
using TransactionService.Controllers.PayersPayees.ViewModels;
using TransactionService.Domain.Models;
using TransactionService.Repositories;
using TransactionService.Services.PayerPayeeEnricher.Exceptions;
using TransactionService.Services.PayerPayeeEnricher.Models;
using TransactionService.Services.PayerPayeeEnricher.Options;

namespace TransactionService.Services.PayerPayeeEnricher
{
    public static class GooglePlacesApiStatus
    {
        public const string NotFound = "NOT_FOUND";
        public const string Ok = "OK";
    }

    public static class GooglePlacesApiFields
    {
        public const string FormattedAddress = "formatted_address";
        public const string PlaceId = "place_id";
    }

    public class GooglePlacesPayerPayeeEnricher : IPayerPayeeEnricher
    {
        private readonly HttpClient _httpClient;
        private readonly GooglePlaceApiOptions _placeApiOptions;
        private readonly ILogger<GooglePlacesPayerPayeeEnricher> _logger;
        private readonly IPayerPayeeRepository _payerPayeeRepository;

        public GooglePlacesPayerPayeeEnricher(HttpClient httpClient, IOptions<GooglePlaceApiOptions> placeApiOptions,
            ILogger<GooglePlacesPayerPayeeEnricher> logger, IPayerPayeeRepository payerPayeeRepository)
        {
            _httpClient = httpClient;
            _logger = logger;
            _payerPayeeRepository = payerPayeeRepository;
            _placeApiOptions = placeApiOptions.Value;
        }

        private async Task<GooglePlaceDetailsResponse> GetGooglePlaceDetails(string placeId, params string[] fields)
        {
            var queryParameters = new Dictionary<string, string>
            {
                { "key", _placeApiOptions.ApiKey },
                { "place_id", placeId },
                { "fields", string.Join(",", fields) }
            };

            var dictFormUrlEncoded = new FormUrlEncodedContent(queryParameters);
            var queryString = await dictFormUrlEncoded.ReadAsStringAsync();
            var url = new Uri(new Uri(_placeApiOptions.PlaceDetailsBaseUri, UriKind.Absolute),
                new Uri($"maps/api/place/details/json?{queryString}", UriKind.Relative));

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new PayerPayeeEnricherException(
                    $"Received an unsuccessful status code from Google Place Details API. Status code: {response.StatusCode}, body: {errorResponse}");
            }

            return await response.Content.ReadFromJsonAsync<GooglePlaceDetailsResponse>();
        }

        private async Task<string> RefreshPlaceId(string placeId)
        {
            var response = await GetGooglePlaceDetails(placeId, GooglePlacesApiFields.PlaceId);

            if (response.Status == GooglePlacesApiStatus.NotFound)
                throw new ExternalLinkIdDefunctException($"Google Place ID {placeId} can no longer be refreshed");

            return response.Result.PlaceId;
        }

        public async Task<ExtraPayerPayeeDetails> GetExtraPayerPayeeDetails(string identifier)
        {
            var placeDetailsResponse = await GetGooglePlaceDetails(identifier, GooglePlacesApiFields.FormattedAddress);

            if (placeDetailsResponse.Status == GooglePlacesApiStatus.NotFound)
            {
                _logger.LogInformation(
                    "Google Place Details response is not found for {GooglePlaceId}, trying to refresh it", identifier);
                var newPlaceId = await RefreshPlaceId(identifier);
                placeDetailsResponse = await GetGooglePlaceDetails(newPlaceId, GooglePlacesApiFields.FormattedAddress);
            }

            return new ExtraPayerPayeeDetails
            {
                Address = placeDetailsResponse.Result.FormattedAddress
            };
        }

        public async Task<PayerPayeeViewModel> EnrichPayerPayeeToViewModel(PayerPayeeType payerPayeeType,
            PayerPayee payerPayeeToBeEnriched)
        {
            if (string.IsNullOrEmpty(payerPayeeToBeEnriched.ExternalId))
                return new PayerPayeeViewModel
                {
                    ExternalId = payerPayeeToBeEnriched.ExternalId,
                    PayerPayeeId = Guid.Parse(payerPayeeToBeEnriched.PayerPayeeId),
                    PayerPayeeName = payerPayeeToBeEnriched.PayerPayeeName,
                };

            var externalId = payerPayeeToBeEnriched.ExternalId;
            var detailsResponse = await GetGooglePlaceDetails(externalId, GooglePlacesApiFields.FormattedAddress);

            if (detailsResponse.Status == GooglePlacesApiStatus.NotFound)
            {
                _logger.LogInformation(
                    "Google Place Details response is not found for {GooglePlaceId}, trying to refresh it", externalId);

                detailsResponse = await GetGooglePlaceDetails(externalId, GooglePlacesApiFields.PlaceId,
                    GooglePlacesApiFields.FormattedAddress);

                if (detailsResponse.Status == GooglePlacesApiStatus.NotFound)
                {
                    _logger.LogInformation(
                        "Google Place Details refresh for {GooglePlaceId} failed, removing it from DB", externalId);
                    await _payerPayeeRepository.PutPayerOrPayee(PayerPayeeType.Payee, payerPayeeToBeEnriched with
                    {
                        ExternalId = ""
                    });
                    return new PayerPayeeViewModel
                    {
                        PayerPayeeId = Guid.Parse(payerPayeeToBeEnriched.PayerPayeeId),
                        PayerPayeeName = payerPayeeToBeEnriched.PayerPayeeName
                    };
                }

                await _payerPayeeRepository.PutPayerOrPayee(PayerPayeeType.Payee, payerPayeeToBeEnriched with
                {
                    ExternalId = detailsResponse.Result.PlaceId
                });
            }

            return new PayerPayeeViewModel
            {
                ExternalId = payerPayeeToBeEnriched.ExternalId,
                PayerPayeeId = Guid.Parse(payerPayeeToBeEnriched.PayerPayeeId),
                PayerPayeeName = payerPayeeToBeEnriched.PayerPayeeName,
                Address = detailsResponse.Result.FormattedAddress
            };
        }
    }
}