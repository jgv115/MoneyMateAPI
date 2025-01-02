using System;
using System.Collections.Generic;
using FluentValidation.TestHelper;
using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Controllers.Transactions.Validators;
using Xunit;

namespace MoneyMateApi.Tests.Controllers.Transactions.Validators;

public class StoreTransactionDtoValidatorTest
{
    private readonly StoreTransactionDtoValidator validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("2021-10-09T09:23:39")]
    [InlineData("2021-10-09")]
    public void GivenInvalidTransactionTimestampThenShouldThrowValidationError(string transactionTimestamp)
    {
        var request = new StoreTransactionDto
        {
            TransactionTimestamp = transactionTimestamp
        };

        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(dto => dto.TransactionTimestamp);
    }

    [Theory]
    [InlineData("2021-10-09T09:23:39Z")]
    [InlineData("2021-10-09T01:23:39.000Z")]
    [InlineData("2022-02-13T13:41:00.000Z")]
    [InlineData("2021-10-09T09:23:39+01")]
    [InlineData("2021-10-09T15:23:39.123+01")]
    [InlineData("2021-10-09T16:23:39+1")]
    [InlineData("2021-10-09T20:23:39+10")]
    [InlineData("2021-10-09T00:23:39+10:00")]
    public void GivenValidTransactionTimestampThenShouldNotThrowValidationError(string transactionTimestamp)
    {
        var request = new StoreTransactionDto
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
        var request = new StoreTransactionDto
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
        var request = new StoreTransactionDto
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
        var request = new StoreTransactionDto
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
        var request = new StoreTransactionDto
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
        var request = new StoreTransactionDto
        {
            Subcategory = subcategory
        };

        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(dto => dto.Category);
    }

    [Fact]
    public void GivenDuplicateTagIds_ThenShouldThrowValidationException()
    {
        var duplicatedTag = Guid.NewGuid();
        var request = new StoreTransactionDto
        {
            TagIds = [duplicatedTag, duplicatedTag, Guid.NewGuid()]
        };

        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(dto => dto.TagIds);
    }
}