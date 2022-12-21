using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using TransactionService.Services.PayerPayeeEnricher;
using TransactionService.Services.PayerPayeeEnricher.Exceptions;
using TransactionService.Services.PayerPayeeEnricher.Models;
using TransactionService.Services.PayerPayeeEnricher.Options;
using Xunit;

namespace TransactionService.Tests.Services.PayerPayeeEnricher;

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

public class GooglePlacesPayerPayeeEnricherTests
{
    [Fact]
    public async Task
        GivenInputIdentifier_WhenGetExtraPayerPayeeDetailsInvoked_ThenCorrectExtraPayerPayeeDetailsReturned()
    {
        const string expectedIdentifier = "external-id-123";
        var googlePlaceOptions = Options.Create(new GooglePlaceApiOptions
        {
            PlaceDetailsBaseUri = "http://base-uri/",
            ApiKey = "key"
        });

        var stubHttpClient = new MockHttpClientBuilder()
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/maps/api/place/details/json?key=key&place_id={expectedIdentifier}&fields=formatted_address",
                HttpStatusCode.OK,
                new GooglePlaceDetailsResponse
                {
                    Result = new()
                    {
                        FormattedAddress = "address123"
                    },
                    Status = "OK"
                })
            .Build();

        var enricher = new GooglePlacesPayerPayeeEnricher(stubHttpClient, googlePlaceOptions);

        var payerPayeeDetails = await enricher.GetExtraPayerPayeeDetails(expectedIdentifier);
        var expectedPayerPayeeDetails = new ExtraPayerPayeeDetails
        {
            Address = "address123"
        };
        Assert.Equal(expectedPayerPayeeDetails, payerPayeeDetails);
    }

    [Fact]
    public async Task GivenInputIdentifierNotFound_WhenGetExtraPayerPayeeDetailsInvoked_ThenNewPlaceIdRequested()
    {
        const string expectedIdentifier = "external-id-123";
        var googlePlaceOptions = Options.Create(new GooglePlaceApiOptions
        {
            PlaceDetailsBaseUri = "http://base-uri/",
            ApiKey = "key"
        });

        var stubHttpClient = new MockHttpClientBuilder()
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/maps/api/place/details/json?key=key&place_id={expectedIdentifier}&fields=formatted_address",
                HttpStatusCode.OK,
                new GooglePlaceDetailsResponse
                {
                    Status = "NOT_FOUND"
                })
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/maps/api/place/details/json?key=key&place_id={expectedIdentifier}&fields=place_id",
                HttpStatusCode.OK,
                new GooglePlaceDetailsResponse
                {
                    Result = new()
                    {
                        PlaceId = "new-place-id-123"
                    }
                })
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/maps/api/place/details/json?key=key&place_id=new-place-id-123&fields=formatted_address",
                HttpStatusCode.OK,
                new GooglePlaceDetailsResponse
                {
                    Result = new()
                    {
                        FormattedAddress = "address123"
                    },
                    Status = "OK"
                })
            .Build();

        var enricher = new GooglePlacesPayerPayeeEnricher(stubHttpClient, googlePlaceOptions);

        var payerPayeeDetails = await enricher.GetExtraPayerPayeeDetails(expectedIdentifier);
        var expectedPayerPayeeDetails = new ExtraPayerPayeeDetails
        {
            Address = "address123"
        };
        Assert.Equal(expectedPayerPayeeDetails, payerPayeeDetails);
    }

    [Fact]
    public async Task
        GivenApiReturnsUnsuccessfulResponse_WhenGetExtraPayerPayeeDetailsInvoked_ThenPayerPayeeEnricherExceptionThrown()
    {
        const string expectedIdentifier = "external-id-123";

        var googlePlaceOptions = Options.Create(new GooglePlaceApiOptions
        {
            PlaceDetailsBaseUri = "http://base-uri/",
            ApiKey = "key"
        });

        var stubHttpClient = new MockHttpClientBuilder()
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/maps/api/place/details/json?key=key&place_id={expectedIdentifier}&fields=formatted_address",
                HttpStatusCode.InternalServerError,
                "failed")
            .Build();

        var enricher = new GooglePlacesPayerPayeeEnricher(stubHttpClient, googlePlaceOptions);

        var thrownException =
            await Assert.ThrowsAsync<PayerPayeeEnricherException>(() =>
                enricher.GetExtraPayerPayeeDetails(expectedIdentifier));

        Assert.Contains("failed", thrownException.Message);
    }
}