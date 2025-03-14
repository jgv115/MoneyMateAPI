using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MoneyMateApi.Connectors.GooglePlaces.Dtos;
using MoneyMateApi.Connectors.GooglePlaces.Exceptions;
using MoneyMateApi.Connectors.GooglePlaces.Models;
using MoneyMateApi.Connectors.GooglePlaces.Options;
using MoneyMateApi.Services.PayerPayeeEnricher.Exceptions;

namespace MoneyMateApi.Connectors.GooglePlaces;

public static class GooglePlacesErrorStatus
{
    public const string NotFound = "NOT_FOUND";
    public const string Ok = "OK";
}

public static class GooglePlacesApiFields
{
    public const string FormattedAddress = "formatted_address";
    public const string PlaceId = "id";
}

public class GooglePlacesConnector : IGooglePlacesConnector
{
    private readonly HttpClient _httpClient;
    private readonly GooglePlaceApiOptions _placeApiOptions;

    public GooglePlacesConnector(HttpClient httpClient, IOptions<GooglePlaceApiOptions> placeApiOptions)
    {
        _httpClient = httpClient;
        _placeApiOptions = placeApiOptions.Value;
    }

    private async Task<(bool Success, GooglePlaceDetailsResponse ResponseBody)> MakeGooglePlaceRequest(string placeId,
        params string[] fields)
    {
        var queryParameters = new Dictionary<string, string>
        {
            { "key", _placeApiOptions.ApiKey },
            { "fields", string.Join(",", fields) }
        };

        var dictFormUrlEncoded = new FormUrlEncodedContent(queryParameters);
        var queryString = await dictFormUrlEncoded.ReadAsStringAsync();
        var url = new Uri(new Uri(_placeApiOptions.PlaceDetailsBaseUri, UriKind.Absolute),
            new Uri($"v1/places/{placeId}?{queryString}", UriKind.Relative));

        var response = await _httpClient.GetAsync(url);
        var responseBody = await response.Content.ReadFromJsonAsync<GooglePlaceDetailsResponse>();

        if (responseBody == null)
            throw new GooglePlacesConnectorException("Response from Google Place API is null");

        return (response.IsSuccessStatusCode, responseBody);
    }


    public async Task<GooglePlaceDetails> GetGooglePlaceDetails(string placeId, params string[] fields)
    {
        var (success, responseBody) = await MakeGooglePlaceRequest(placeId, fields);
        if (success) return responseBody.ToGooglePlaceDetails(placeId);

        // Error path
        if (responseBody.Error?.Status == GooglePlacesErrorStatus.NotFound)
        {
            var refreshResponse = await MakeGooglePlaceRequest(placeId, GooglePlacesApiFields.PlaceId);

            if (refreshResponse.Success)
            {
                var refreshedId = refreshResponse.ResponseBody.Id;

                if (refreshedId is null)
                    throw new GooglePlacesConnectorException(
                        "Refreshed placeId returned as null, not sure what happened");

                return (await MakeGooglePlaceRequest(refreshedId, fields)).ResponseBody.ToGooglePlaceDetails(
                    refreshedId);
            }


            if (refreshResponse.ResponseBody.Error?.Status == GooglePlacesErrorStatus.NotFound)
                throw new ExternalLinkIdDefunctException($"Google Place ID {placeId} can no longer be refreshed");
        }

        throw new GooglePlacesConnectorException(
            $"Received an unsuccessful status code from Google Place Details API. Status code: {responseBody.Error?.Status}, body: {JsonSerializer.Serialize(responseBody)}");
    }
}