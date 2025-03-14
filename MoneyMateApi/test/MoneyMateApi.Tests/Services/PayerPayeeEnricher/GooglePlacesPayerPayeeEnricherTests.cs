using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoneyMateApi.Connectors.GooglePlaces;
using MoneyMateApi.Connectors.GooglePlaces.Models;
using Moq;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.PayersPayees.ViewModels;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Repositories;
using MoneyMateApi.Services.PayerPayeeEnricher;
using MoneyMateApi.Services.PayerPayeeEnricher.Exceptions;
using Xunit;

namespace MoneyMateApi.Tests.Services.PayerPayeeEnricher;

public class GooglePlacesPayerPayeeEnricherTests
{
    private readonly Mock<ILogger<GooglePlacesPayerPayeeEnricher>> _mockLogger = new();
    private readonly Mock<IPayerPayeeRepository> _mockPayerPayeeRepository = new();
    private readonly Mock<IGooglePlacesConnector> _mockGooglePlacesConnector = new();

    [Fact]
    public async Task
        GivenPayerOrPayeeWithNoExternalId_WhenEnrichAndMapPayerPayeeToViewModelInvoked_ThenViewModelReturnedWithNoAdditionalInfo()
    {
        var enricher =
            new GooglePlacesPayerPayeeEnricher(_mockLogger.Object, _mockPayerPayeeRepository.Object,
                _mockGooglePlacesConnector.Object);

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

        _mockGooglePlacesConnector.Setup(connector =>
            connector.GetGooglePlaceDetails(expectedIdentifier, "formatted_address")).ReturnsAsync(() =>
            new GooglePlaceDetails
            {
                Id = expectedIdentifier,
                FormattedAddress = "address123"
            });

        var enricher =
            new GooglePlacesPayerPayeeEnricher(_mockLogger.Object, _mockPayerPayeeRepository.Object,
                _mockGooglePlacesConnector.Object);

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
        const string oldPlaceId = "external-id-123";
        const string newPlaceId = "new-place-id";

        _mockGooglePlacesConnector.Setup(connector =>
            connector.GetGooglePlaceDetails(oldPlaceId, "formatted_address")).ReturnsAsync(() =>
            new GooglePlaceDetails
            {
                Id = newPlaceId,
                FormattedAddress = "address123"
            });

        var initialPayerPayee = new PayerPayee
        {
            ExternalId = oldPlaceId,
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name"
        };

        var expectedNewPayerPayee = initialPayerPayee with
        {
            ExternalId = newPlaceId
        };
        _mockPayerPayeeRepository.Setup(repository => repository.PutPayerOrPayee(PayerPayeeType.Payee,
            expectedNewPayerPayee));

        var enricher =
            new GooglePlacesPayerPayeeEnricher(_mockLogger.Object, _mockPayerPayeeRepository.Object,
                _mockGooglePlacesConnector.Object);

        var returnedPayeeViewModel =
            await enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, initialPayerPayee);

        Assert.Equal(new PayerPayeeViewModel
        {
            ExternalId = newPlaceId,
            PayerPayeeId = Guid.Parse(initialPayerPayee.PayerPayeeId),
            PayerPayeeName = initialPayerPayee.PayerPayeeName,
            Address = "address123"
        }, returnedPayeeViewModel);

        _mockPayerPayeeRepository.VerifyAll();
    }

    [Fact]
    public async Task
        GivenInputIdentifierThatIsDefunct_WhenEnrichAndMapPayerPayeeToViewModelInvoked_ThenExternalIdRemovedFromDbAndCorrectModelReturned()
    {
        const string expectedIdentifier = "external-id-123";

        _mockGooglePlacesConnector.Setup(connector =>
                connector.GetGooglePlaceDetails(expectedIdentifier, "formatted_address"))
            .ThrowsAsync(new ExternalLinkIdDefunctException("defunct!"));

        var initialPayerPayee = new PayerPayee
        {
            ExternalId = expectedIdentifier,
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name"
        };

        _mockPayerPayeeRepository.Setup(repository => repository.PutPayerOrPayee(PayerPayeeType.Payer, new PayerPayee
        {
            ExternalId = "",
            PayerPayeeId = initialPayerPayee.PayerPayeeId,
            PayerPayeeName = initialPayerPayee.PayerPayeeName
        }));

        var enricher =
            new GooglePlacesPayerPayeeEnricher(_mockLogger.Object, _mockPayerPayeeRepository.Object,
                _mockGooglePlacesConnector.Object);

        var returnedViewModel = await enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payer, initialPayerPayee);

        _mockPayerPayeeRepository.VerifyAll();

        Assert.Equal(new PayerPayeeViewModel
        {
            PayerPayeeId = Guid.Parse(initialPayerPayee.PayerPayeeId),
            PayerPayeeName = "name"
        }, returnedViewModel);
    }
}