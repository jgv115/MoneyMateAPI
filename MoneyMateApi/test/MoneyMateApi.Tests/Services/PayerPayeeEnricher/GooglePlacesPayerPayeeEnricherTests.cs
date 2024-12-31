using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.PayersPayees.ViewModels;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Repositories;
using MoneyMateApi.Services.PayerPayeeEnricher;
using MoneyMateApi.Services.PayerPayeeEnricher.Exceptions;
using MoneyMateApi.Services.PayerPayeeEnricher.Models;
using MoneyMateApi.Services.PayerPayeeEnricher.Options;
using Xunit;

namespace MoneyMateApi.Tests.Services.PayerPayeeEnricher;

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
    private readonly Mock<ILogger<GooglePlacesPayerPayeeEnricher>> _mockLogger = new();
    private readonly Mock<IPayerPayeeRepository> _mockPayerPayeeRepository = new();

    private IOptions<GooglePlaceApiOptions> GetStubGooglePlaceApiOptions()
    {
        return Options.Create(new GooglePlaceApiOptions
        {
            PlaceDetailsBaseUri = "http://base-uri/",
            ApiKey = "key"
        });
    }

    [Fact]
    public async Task
        GivenPayerOrPayeeWithNoExternalId_WhenEnrichAndMapPayerPayeeToViewModelInvoked_ThenViewModelReturnedWithNoAdditionalInfo()
    {
        var stubHttpClient = new MockHttpClientBuilder().Build();
        var enricher =
            new GooglePlacesPayerPayeeEnricher(stubHttpClient, GetStubGooglePlaceApiOptions(), _mockLogger.Object,
                _mockPayerPayeeRepository.Object);

        var initialPayerPayee = new PayerPayee
        {
            ExternalId = "",
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name"
        };
        var returnedViewModel = await enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, initialPayerPayee);

        Assert.Equal(new PayerPayeeViewModel()
        {
            ExternalId = initialPayerPayee.ExternalId,
            PayerPayeeId = Guid.Parse(initialPayerPayee.PayerPayeeId),
            PayerPayeeName = initialPayerPayee.PayerPayeeName,
        }, returnedViewModel);
    }

    [Fact]
    public async Task
        GivenPayerOrPayeeWithExternalId_WhenEnrichAndMapPayerPayeeToViewModelInvoked_ThenCorrectPayerPayeeViewModelReturned()
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

        var enricher = new GooglePlacesPayerPayeeEnricher(stubHttpClient, googlePlaceOptions, _mockLogger.Object,
            _mockPayerPayeeRepository.Object);

        var initialPayerPayee = new PayerPayee
        {
            ExternalId = expectedIdentifier,
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name"
        };
        var returnedPayeeViewModel =
            await enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, initialPayerPayee);

        Assert.Equal(new PayerPayeeViewModel
        {
            ExternalId = initialPayerPayee.ExternalId,
            PayerPayeeId = Guid.Parse(initialPayerPayee.PayerPayeeId),
            PayerPayeeName = initialPayerPayee.PayerPayeeName,
            Address = "address123"
        }, returnedPayeeViewModel);
    }

    [Fact]
    public async Task
        GivenInputIdentifierNotFound_WhenEnrichAndMapPayerPayeeToViewModelInvoked_ThenAddressWithNewPlaceIdRequestedAndStored()
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
                    Status = GooglePlacesApiStatus.NotFound
                })
            .SetupMockResponse(HttpMethod.Get,
                $"http://base-uri/maps/api/place/details/json?key=key&place_id={expectedIdentifier}&fields=place_id%2Cformatted_address",
                HttpStatusCode.OK,
                new GooglePlaceDetailsResponse
                {
                    Result = new()
                    {
                        PlaceId = "new-place-id-123",
                        FormattedAddress = "address123"
                    },
                    Status = GooglePlacesApiStatus.Ok
                })
            .Build();

        var initialPayerPayee = new PayerPayee
        {
            ExternalId = expectedIdentifier,
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name"
        };

        var expectedNewPayerPayee = initialPayerPayee with
        {
            ExternalId = "new-place-id-123"
        };
        _mockPayerPayeeRepository.Setup(repository => repository.PutPayerOrPayee(PayerPayeeType.Payee,
            expectedNewPayerPayee));

        var enricher = new GooglePlacesPayerPayeeEnricher(stubHttpClient, googlePlaceOptions, _mockLogger.Object,
            _mockPayerPayeeRepository.Object);

        var returnedPayeeViewModel =
            await enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, initialPayerPayee);

        Assert.Equal(new PayerPayeeViewModel
        {
            ExternalId = initialPayerPayee.ExternalId,
            PayerPayeeId = Guid.Parse(initialPayerPayee.PayerPayeeId),
            PayerPayeeName = initialPayerPayee.PayerPayeeName,
            Address = "address123"
        }, returnedPayeeViewModel);

        _mockPayerPayeeRepository.VerifyAll();
    }

    [Fact]
    public async Task
        GivenInputIdentifierThatCannotBeRefreshed_WhenEnrichAndMapPayerPayeeToViewModelInvoked_ThenExternalIdRemovedFromDbAndCorrectModelReturned()
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
                $"http://base-uri/maps/api/place/details/json?key=key&place_id={expectedIdentifier}&fields=place_id%2Cformatted_address",
                HttpStatusCode.OK,
                new GooglePlaceDetailsResponse
                {
                    Status = "NOT_FOUND",
                })
            .Build();

        var initialPayerPayee = new PayerPayee
        {
            ExternalId = expectedIdentifier,
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name"
        };

        _mockPayerPayeeRepository.Setup(repository => repository.PutPayerOrPayee(PayerPayeeType.Payee, new PayerPayee
        {
            ExternalId = "",
            PayerPayeeId = initialPayerPayee.PayerPayeeId,
            PayerPayeeName = initialPayerPayee.PayerPayeeName
        }));

        var enricher = new GooglePlacesPayerPayeeEnricher(stubHttpClient, googlePlaceOptions, _mockLogger.Object,
            _mockPayerPayeeRepository.Object);

        var returnedViewModel = await enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, initialPayerPayee);

        _mockPayerPayeeRepository.VerifyAll();

        Assert.Equal(new PayerPayeeViewModel
        {
            PayerPayeeId = Guid.Parse(initialPayerPayee.PayerPayeeId),
            PayerPayeeName = "name"
        }, returnedViewModel);
    }

    [Fact]
    public async Task
        GivenApiReturnsUnsuccessfulResponse_WhenEnrichAndMapPayerPayeeToViewModelInvoked_ThenPayerPayeeEnricherExceptionThrown()
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

        var enricher = new GooglePlacesPayerPayeeEnricher(stubHttpClient, googlePlaceOptions, _mockLogger.Object,
            _mockPayerPayeeRepository.Object);

        var thrownException =
            await Assert.ThrowsAsync<PayerPayeeEnricherException>(() =>
                enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, new PayerPayee
                {
                    ExternalId = expectedIdentifier,
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name"
                }));

        Assert.Contains("failed", thrownException.Message);
    }
}