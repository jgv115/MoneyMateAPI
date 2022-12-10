using System.Threading.Tasks;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Places.Details.Request.Enums;
using GoogleApi.Interfaces.Places;
using TransactionService.Services.PayerPayeeEnricher.Models;

namespace TransactionService.Services.PayerPayeeEnricher
{
    public class GooglePlacesPayerPayeeEnricher : IPayerPayeeEnricher
    {
        private readonly IDetailsApi _detailsApi;

        public GooglePlacesPayerPayeeEnricher(IDetailsApi detailsApi)
        {
            _detailsApi = detailsApi;
        }

        private async Task<string> _refreshPlaceId(string placeId)
        {
            var test = await _detailsApi.QueryAsync(new()
            {
                PlaceId = placeId,
                Fields = FieldTypes.Place_Id
            });

            return test.Result.PlaceId;
        }

        public async Task<ExtraPayerPayeeDetails> GetExtraPayerPayeeDetails(string identifier)
        {
            var details = await _detailsApi.QueryAsync(new()
            {
                PlaceId = identifier,
                Fields = FieldTypes.Address_Component,
            });

            if (details.Status == Status.NotFound)
            {
                var newPlaceId = await _refreshPlaceId(identifier);
                details = await _detailsApi.QueryAsync(new()
                {
                    PlaceId = newPlaceId,
                    Fields = FieldTypes.Address_Component,
                });
            }

            return new ExtraPayerPayeeDetails
            {
                Address = details.Result.FormattedAddress
            };
        }
    }
}