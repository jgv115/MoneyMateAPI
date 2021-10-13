using System.Collections.Generic;
using System;
using TransactionService.Dtos;
using TransactionService.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace TransactionService.Tests.Validators
{
    public class PutTransactionDtoValidatorTests
    {
        private readonly PutTransactionDtoValidator validator = new();

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("2021-10-09T09:23:39")]
        [InlineData("2021-10-09")]
        public void GivenInvalidTransactionTimestampThenShouldThrowValidationError(string transactionTimestamp)
        {
            var request = new PutTransactionDto
            {
                TransactionTimestamp = transactionTimestamp
            };

            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(dto => dto.TransactionTimestamp);
        }

        [Theory]
        [InlineData("2021-10-09T09:23:39Z")]
        [InlineData("2021-10-09T09:23:39+01")]
        [InlineData("2021-10-09T09:23:39+1")]
        [InlineData("2021-10-09T09:23:39+10")]
        [InlineData("2021-10-09T09:23:39+10:00")]
        public void GivenValidTransactionTimestampThenShouldNotThrowValidationError(string transactionTimestamp)
        {
            var request = new PutTransactionDto
            {
                TransactionTimestamp = transactionTimestamp
            };

            var result = validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(dto => dto.TransactionTimestamp);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("not expense")]
        public void GivenInvalidTransactionType_ThenShouldThrowValidationError(string transactionType)
        {
            var request = new PutTransactionDto
            {
                TransactionType = transactionType
            };

            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(dto => dto.TransactionType);
        }

        [Theory]
        [InlineData("expense")]
        [InlineData("income")]
        public void GivenValidTransactionType_ThenShouldNotThrowValidationError(string transactionType)
        {
            var request = new PutTransactionDto
            {
                TransactionType = transactionType
            };

            var result = validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(dto => dto.TransactionType);
        }

        [Theory]
        [InlineData(0)]
        public void GivenInvalidAmount_ThenShouldThrowValidationException(decimal amount)
        {
            var request = new PutTransactionDto
            {
                Amount = amount
            };

            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(dto => dto.Amount);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GivenInvalidCategory_ThenShouldThrowValidationException(string category)
        {
            var request = new PutTransactionDto
            {
                Category = category
            };

            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(dto => dto.Category);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GivenInvalidSubcategory_ThenShouldThrowValidationException(string subcategory)
        {
            var request = new PutTransactionDto
            {
                SubCategory = subcategory
            };

            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(dto => dto.Category);
        }
    }
}