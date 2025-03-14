using System.Threading.Tasks;
using MoneyMateApi.Connectors.GooglePlaces.Models;

namespace MoneyMateApi.Connectors.GooglePlaces;

public interface IGooglePlacesConnector
{
    Task<GooglePlaceDetails> GetGooglePlaceDetails(string placeId, params string[] fields);
}