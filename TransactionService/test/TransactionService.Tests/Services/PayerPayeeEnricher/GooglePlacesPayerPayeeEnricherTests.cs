using System.Threading;
using System.Threading.Tasks;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Places.Details.Request;
using GoogleApi.Entities.Places.Details.Request.Enums;
using GoogleApi.Entities.Places.Details.Response;
using GoogleApi.Interfaces.Places;
using Moq;
using TransactionService.Services.PayerPayeeEnricher;
using TransactionService.Services.PayerPayeeEnricher.Models;
using Xunit;

namespace TransactionService.Tests.Services.PayerPayeeEnricher;

public class GooglePlacesPayerPayeeEnricherTests
{
    private readonly Mock<IDetailsApi> _mockDetailsApi = new();

    [Fact]
    public async Task
        GivenInputIdentifier_WhenGetExtraPayerPayeeDetailsInvoked_ThenCorrectExtraPayerPayeeDetailsReturned()
    {
        var enricher = new GooglePlacesPayerPayeeEnricher(_mockDetailsApi.Object);

        const string expectedPlaceId = "placeId123";
        _mockDetailsApi.Setup(detailsApi => detailsApi
            .QueryAsync(It.Is<PlacesDetailsRequest>(request =>
                    request.PlaceId == expectedPlaceId && request.Fields == FieldTypes.Address_Component),
                It.IsAny<CancellationToken>())).ReturnsAsync(
            () => new PlacesDetailsResponse
            {
                Result = new DetailsResult
                {
                    FormattedAddress = "output address"
                }
            });

        var payerPayeeDetails = await enricher.GetExtraPayerPayeeDetails(expectedPlaceId);

        var expectedPayerPayeeDetails = new ExtraPayerPayeeDetails
        {
            Address = "output address"
        };

        Assert.Equal(expectedPayerPayeeDetails, payerPayeeDetails);
    }

    [Fact]
    public async Task GivenInputIdentifierNotFound_WhenGetExtraPayerPayeeDetailsInvoked_ThenNewPlaceIdRequested()
    {
        var enricher = new GooglePlacesPayerPayeeEnricher(_mockDetailsApi.Object);

        _mockDetailsApi.Setup(detailsApi => detailsApi
            .QueryAsync(It.Is<PlacesDetailsRequest>(request =>
                    request.PlaceId == "inputPlaceId" && request.Fields == FieldTypes.Address_Component),
                It.IsAny<CancellationToken>())).ReturnsAsync(
            () => new PlacesDetailsResponse
            {
                Status = Status.NotFound
            });


        _mockDetailsApi.Setup(detailsApi => detailsApi
            .QueryAsync(It.Is<PlacesDetailsRequest>(request =>
                    request.PlaceId == "inputPlaceId" && request.Fields == FieldTypes.Place_Id),
                It.IsAny<CancellationToken>())).ReturnsAsync(
            () => new PlacesDetailsResponse
            {
                Result = new()
                {
                    PlaceId = "new placeId"
                }
            });

        _mockDetailsApi.Setup(detailsApi => detailsApi
            .QueryAsync(It.Is<PlacesDetailsRequest>(request =>
                    request.PlaceId == "new placeId" && request.Fields == FieldTypes.Address_Component),
                It.IsAny<CancellationToken>())).ReturnsAsync(
            () => new PlacesDetailsResponse
            {
                Result = new DetailsResult
                {
                    FormattedAddress = "output address"
                }
            });

        var payerPayeeDetails = await enricher.GetExtraPayerPayeeDetails("inputPlaceId");

        var expectedPayerPayeeDetails = new ExtraPayerPayeeDetails
        {
            Address = "output address"
        };

        Assert.Equal(expectedPayerPayeeDetails, payerPayeeDetails);
    }
}