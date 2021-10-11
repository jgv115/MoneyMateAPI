using System.Globalization;
using System;
using FluentValidation;
using TransactionService.Dtos;
using System.Collections.Generic;

namespace TransactionService.Validators
{
    public class StoreTransactionDtoValidator : AbstractValidator<StoreTransactionDto>
    {
        public StoreTransactionDtoValidator()
        {
            RuleFor(dto => dto.TransactionTimestamp).NotEmpty().Must(timestamp =>
            {
                var validFormats = new[]
                {
                    "yyyy-MM-ddThh:mm:ssZ",
                    "yyyy-MM-ddThh:mm:ssz",
                    "yyyy-MM-ddThh:mm:sszz",
                    "yyyy-MM-ddThh:mm:sszzz"
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
            RuleFor(dto => dto.SubCategory).NotEmpty();
        }
    }
}