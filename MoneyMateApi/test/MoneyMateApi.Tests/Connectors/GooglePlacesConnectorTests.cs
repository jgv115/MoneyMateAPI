using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MoneyMateApi.Connectors.GooglePlaces;
using MoneyMateApi.Connectors.GooglePlaces.Dtos;
using MoneyMateApi.Connectors.GooglePlaces.Exceptions;
using MoneyMateApi.Connectors.GooglePlaces.Models;
using MoneyMateApi.Connectors.GooglePlaces.Options;
using MoneyMateApi.Services.PayerPayeeEnricher.Exceptions;
using Moq;
using Moq.Protected;
using Xunit;

namespace MoneyMateApi.Tests.Connectors;

public class MockHttpClientBuilder
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public MockHttpClientBuilder()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
    }

    public MockHttpClientBuilder SetupMockResponse<TResponse>(
        HttpMethod method,
        string uri,
        HttpStatusCode responseStatusCode,
        TResponse responseObject
    )
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(message =>
                    message.Method == HttpMethod.Get &&
                    message.RequestUri.ToString() == uri),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = responseStatusCode,
                Content = new StringContent(JsonSerializer.Serialize(responseObject))
            });

        return this;
    }

    public HttpClient Build() => new(_httpMessageHandlerMock.Object);
}

public class GooglePlacesConnectorTests
{
    private IOptions<GooglePlaceApiOptions> GetStubGooglePlaceApiOptions()
    {
        return Options.Create(new GooglePlaceApiOptions
        {
            PlaceDetailsBaseUri = "http://base-uri/",
            ApiKey = "key"
        });
    }

    [Fact]
    public async Task GivenPlaceIdAndFields_WhenGetGooglePlaceDetailsInvoked_ThenCorrectResponseIsReturnedOnSuccess()
    {
        const string expectedIdentifier = "external-id-123";

        var stubHttpClient = new MockHttpClientBuilder()
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/v1/places/{expectedIdentifier}?key=key&fields=formatted_address",
                HttpStatusCode.OK,
                new GooglePlaceDetailsResponse
                {
                    FormattedAddress = "address123"
                })
            .Build();

        var connector = new GooglePlacesConnector(stubHttpClient, GetStubGooglePlaceApiOptions());

        var response = await connector.GetGooglePlaceDetails(expectedIdentifier, "formatted_address");

        Assert.Equal(new GooglePlaceDetails
        {
            Id = expectedIdentifier,
            FormattedAddress = "address123"
        }, response);
    }

    [Fact]
    public async Task GivenPlaceIdAndFields_WhenGetGooglePlaceDetailsInvoked_ThenExceptionThrownWhenResponseBodyIsNull()
    {
        const string expectedIdentifier = "external-id-123";

        var stubHttpClient = new MockHttpClientBuilder()
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/v1/places/{expectedIdentifier}?key=key&fields=formatted_address",
                HttpStatusCode.OK,
                (GooglePlaceDetailsResponse)null!)
            .Build();

        var connector = new GooglePlacesConnector(stubHttpClient, GetStubGooglePlaceApiOptions());

        await Assert.ThrowsAsync<GooglePlacesConnectorException>(() =>
            connector.GetGooglePlaceDetails(expectedIdentifier, "formatted_address"));
    }

    [Fact]
    public async Task
        GivenExpiredPlaceIdAndFields_WhenGetGooglePlaceDetailsInvoked_ThenResponseReturnedWithRefreshedId()
    {
        const string oldExternalId = "external-id-123";
        const string newExternalId = "new-external-id-123";

        var stubHttpClient = new MockHttpClientBuilder()
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/v1/places/{oldExternalId}?key=key&fields=formatted_address",
                HttpStatusCode.NotFound,
                new GooglePlaceDetailsResponse
                {
                    Error = new GooglePlaceDetailsError
                    {
                        Status = "NOT_FOUND",
                        Message = "The provided Place ID is no longer valid."
                    }
                })
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/v1/places/{oldExternalId}?key=key&fields=id",
                HttpStatusCode.OK,
                new GooglePlaceDetailsResponse
                {
                    Id = newExternalId,
                })
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/v1/places/{newExternalId}?key=key&fields=formatted_address",
                HttpStatusCode.OK,
                new GooglePlaceDetailsResponse
                {
                    FormattedAddress = "address123"
                })
            .Build();

        var connector = new GooglePlacesConnector(stubHttpClient, GetStubGooglePlaceApiOptions());

        var response = await connector.GetGooglePlaceDetails(oldExternalId, "formatted_address");
        Assert.Equal(new GooglePlaceDetails
        {
            Id = newExternalId,
            FormattedAddress = "address123"
        }, response);
    }

    [Fact]
    public async Task
        GivenExpiredPlaceIdAndFields_WhenGetGooglePlaceDetailsInvoked_ThenDefunctExceptionThrownIfIdCanNoLongerBeRefreshed()
    {
        const string oldExternalId = "external-id-123";

        var stubHttpClient = new MockHttpClientBuilder()
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/v1/places/{oldExternalId}?key=key&fields=formatted_address",
                HttpStatusCode.NotFound,
                new GooglePlaceDetailsResponse
                {
                    Error = new GooglePlaceDetailsError
                    {
                        Status = "NOT_FOUND",
                        Message = "The provided Place ID is no longer valid."
                    }
                })
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/v1/places/{oldExternalId}?key=key&fields=id",
                HttpStatusCode.NotFound,
                new GooglePlaceDetailsResponse
                {
                    Error = new GooglePlaceDetailsError
                    {
                        Status = "NOT_FOUND",
                        Message = "The provided Place ID is no longer valid."
                    }
                })

            .Build();

        var connector = new GooglePlacesConnector(stubHttpClient, GetStubGooglePlaceApiOptions());

        var exception = await Assert.ThrowsAsync<ExternalLinkIdDefunctException>(() =>
            connector.GetGooglePlaceDetails(oldExternalId, "formatted_address"));

        Assert.Equal($"Google Place ID {oldExternalId} can no longer be refreshed", exception.Message);
    }

    [Fact]
    public async Task
        GivenExpiredPlaceIdAndFields_WhenGetGooglePlaceDetailsInvoked_ThenExceptionThrownIfRefreshedIdIsNull()
    {
        const string oldExternalId = "external-id-123";

        var stubHttpClient = new MockHttpClientBuilder()
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/v1/places/{oldExternalId}?key=key&fields=formatted_address",
                HttpStatusCode.NotFound,
                new GooglePlaceDetailsResponse
                {
                    Error = new GooglePlaceDetailsError
                    {
                        Status = "NOT_FOUND",
                        Message = "The provided Place ID is no longer valid."
                    }
                })
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/v1/places/{oldExternalId}?key=key&fields=id",
                HttpStatusCode.OK,
                new GooglePlaceDetailsResponse
                {
                    Id = null
                })
            .Build();

        var connector = new GooglePlacesConnector(stubHttpClient, GetStubGooglePlaceApiOptions());

        var exception = await Assert.ThrowsAsync<GooglePlacesConnectorException>(() =>
            connector.GetGooglePlaceDetails(oldExternalId, "formatted_address"));

        Assert.Equal("Refreshed placeId returned as null, not sure what happened", exception.Message);
    }

    [Fact]
    public async Task
        GivenPlaceIdAndFields_WhenGetGooglePlaceDetailsInvoked_ThenExceptionThrownWhenUnknownErrorIsReturned()
    {
        const string expectedIdentifier = "external-id-123";

        var stubHttpClient = new MockHttpClientBuilder()
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/v1/places/{expectedIdentifier}?key=key&fields=formatted_address",
                HttpStatusCode.NotFound,
                new GooglePlaceDetailsResponse
                {
                    Error = new GooglePlaceDetailsError
                    {
                        Status = "unknown",
                        Message = "error"
                    }
                })
            .Build();

        var connector = new GooglePlacesConnector(stubHttpClient, GetStubGooglePlaceApiOptions());

        var exception = await Assert.ThrowsAsync<GooglePlacesConnectorException>(() =>
            connector.GetGooglePlaceDetails(expectedIdentifier, "formatted_address"));

        Assert.Contains($"Received an unsuccessful status code from Google Place Details API. Status code: unknown",
            exception.Message);
    }
}