using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoneyMateApi.Connectors.GooglePlaces;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.PayersPayees.ViewModels;
using MoneyMateApi.Domain.PayerPayees;
using MoneyMateApi.Repositories;
using MoneyMateApi.Services.PayerPayeeEnricher.Exceptions;

namespace MoneyMateApi.Services.PayerPayeeEnricher
{
    public class GooglePlacesPayerPayeeEnricher : IPayerPayeeEnricher
    {
        private readonly ILogger<GooglePlacesPayerPayeeEnricher> _logger;
        private readonly IPayerPayeeRepository _payerPayeeRepository;
        private readonly IGooglePlacesConnector _googlePlacesConnector;

        public GooglePlacesPayerPayeeEnricher(
            ILogger<GooglePlacesPayerPayeeEnricher> logger, IPayerPayeeRepository payerPayeeRepository,
            IGooglePlacesConnector googlePlacesConnector)
        {
            _logger = logger;
            _payerPayeeRepository = payerPayeeRepository;
            _googlePlacesConnector = googlePlacesConnector;
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

            try
            {
                var detailsResponse =
                    await _googlePlacesConnector.GetGooglePlaceDetails(externalId,
                        GooglePlacesApiFields.FormattedAddress);

                if (detailsResponse.Id != externalId)
                {
                    await _payerPayeeRepository.PutPayerOrPayee(payerPayeeType, payerPayeeToBeEnriched with
                    {
                        ExternalId = detailsResponse.Id
                    });
                    
                    return new PayerPayeeViewModel
                    {
                        ExternalId = detailsResponse.Id,
                        PayerPayeeId = Guid.Parse(payerPayeeToBeEnriched.PayerPayeeId),
                        PayerPayeeName = payerPayeeToBeEnriched.PayerPayeeName,
                        Address = detailsResponse.FormattedAddress
                    };
                }

                return new PayerPayeeViewModel
                {
                    ExternalId = payerPayeeToBeEnriched.ExternalId,
                    PayerPayeeId = Guid.Parse(payerPayeeToBeEnriched.PayerPayeeId),
                    PayerPayeeName = payerPayeeToBeEnriched.PayerPayeeName,
                    Address = detailsResponse.FormattedAddress
                };
            }
            catch (ExternalLinkIdDefunctException)
            {
                _logger.LogInformation(
                    "Google Place Details refresh for {GooglePlaceId} failed, removing it from DB", externalId);
                await _payerPayeeRepository.PutPayerOrPayee(payerPayeeType, payerPayeeToBeEnriched with
                {
                    ExternalId = ""
                });
                return new PayerPayeeViewModel
                {
                    PayerPayeeId = Guid.Parse(payerPayeeToBeEnriched.PayerPayeeId),
                    PayerPayeeName = payerPayeeToBeEnriched.PayerPayeeName
                };
            }
        }
    }
}