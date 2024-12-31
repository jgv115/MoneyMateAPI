using System;

namespace MoneyMateApi.Services.PayerPayeeEnricher.Exceptions
{
    public class ExternalLinkIdDefunctException : Exception
    {
        public ExternalLinkIdDefunctException(string message) : base(message)
        {
        }
    }
}