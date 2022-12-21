using System;

namespace TransactionService.Services.PayerPayeeEnricher.Exceptions
{
    public class PayerPayeeEnricherException : Exception
    {
        public PayerPayeeEnricherException(string message) : base(message)
        {
        }
    }
}