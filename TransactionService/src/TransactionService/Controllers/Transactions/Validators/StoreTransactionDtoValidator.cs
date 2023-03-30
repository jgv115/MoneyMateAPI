using System;
using System.Collections.Generic;
using System.Globalization;
using FluentValidation;
using TransactionService.Controllers.Transactions.Dtos;

namespace TransactionService.Controllers.Transactions.Validators
{
    public class StoreTransactionDtoValidator : AbstractValidator<StoreTransactionDto>
    {
        public StoreTransactionDtoValidator()
        {
            RuleFor(dto => dto.TransactionTimestamp).NotEmpty().Must(timestamp =>
            {
                var validFormats = new[]
                {
                    "yyyy-MM-ddTHH:mm:ssZ",
                    "yyyy-MM-ddTHH:mm:ss.FZ",
                    "yyyy-MM-ddTHH:mm:ss.FFZ",
                    "yyyy-MM-ddTHH:mm:ss.FFFZ",
                    "yyyy-MM-ddTHH:mm:ssz",
                    "yyyy-MM-ddTHH:mm:ss.FFFz",
                    "yyyy-MM-ddTHH:mm:sszz",
                    "yyyy-MM-ddTHH:mm:ss.FFFzz",
                    "yyyy-MM-ddTHH:mm:sszzz",
                    "yyyy-MM-ddTHH:mm:ssFFFzzz"
                };

                DateTime dt;
                var valid = DateTime.TryParseExact(
                    timestamp,
                    validFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out dt
                );

                return valid;
            });
            RuleFor(dto => dto.TransactionType).NotEmpty().Must(transactionType =>
            {
                var validTransactionTypes = new List<string> { "expense", "income" };
                if (validTransactionTypes.Contains(transactionType))
                    return true;
                return false;
            });
            RuleFor(dto => dto.Amount).NotEmpty();
            RuleFor(dto => dto.Category).NotEmpty();
            RuleFor(dto => dto.Subcategory).NotEmpty();
        }
    }
}