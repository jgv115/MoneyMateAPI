using System;

namespace MoneyMateApi.Connectors.GooglePlaces.Exceptions
{
    public class GooglePlacesConnectorException : Exception
    {
        public GooglePlacesConnectorException(string message) : base(message)
        {
        }
    }
}