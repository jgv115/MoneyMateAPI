using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentValidation;
using MoneyMateApi.Controllers.Transactions.Dtos;

namespace MoneyMateApi.Controllers.Transactions.Validators
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
                return validTransactionTypes.Contains(transactionType);
            });
            RuleFor(dto => dto.Amount).NotEmpty();
            RuleFor(dto => dto.Category).NotEmpty();
            RuleFor(dto => dto.Subcategory).NotEmpty();
            RuleFor(dto => dto.TagIds).Must(tagIds => tagIds.Distinct().Count() == tagIds.Count)
                .WithMessage("TagIds must be unique");
        }
    }
}